using System;
using System.Collections.Generic;
using TalismanBag.Combat;
using TalismanBag.Grid;
using TalismanBag.Inventory;
using TalismanBag.Items;
using TalismanBag.UI;
using TalismanBag.V02.CoreLoop.Save;
using UnityEngine;

namespace TalismanBag.V02.Rewards
{
    public sealed class V02RewardInventoryAdapter : MonoBehaviour
    {
        [SerializeField] private PlayerTalismanInventory inventory;
        [SerializeField] private TalismanBagGrid grid;
        [SerializeField] private Canvas rootCanvas;
        [SerializeField] private AutoCombatController combatController;
        [SerializeField] private Transform inventoryParent;
        [SerializeField] private DraggableTalismanItemView itemViewTemplate;

        private readonly Dictionary<string, TalismanItemDefinition> definitionCatalog = new();
        private DraggableTalismanItemView runtimeTemplate;

        public DraggableTalismanItemView AddTalisman(TalismanItemDefinition definition, int level = 1)
        {
            DraggableTalismanItemView template = GetTemplate();
            if (definition == null || template == null || inventoryParent == null)
            {
                return null;
            }

            CacheDefinition(definition);
            TalismanItemRuntime runtime = inventory != null
                ? inventory.AddItem(definition, level)
                : new TalismanItemRuntime(definition, level);

            DraggableTalismanItemView view = Instantiate(template, inventoryParent);
            view.name = $"RewardItem_{definition.itemId}";
            view.gameObject.SetActive(true);
            view.BindRuntime(runtime, grid, rootCanvas, combatController);
            view.CaptureHome();
            combatController?.RegisterItemView(view);
            return view;
        }

        public void ResetTalismansByIds(IEnumerable<string> itemIds)
        {
            CacheKnownDefinitions();
            GetTemplate();

            combatController?.ClearRegisteredItemViewsForInventoryReset();
            inventory?.ResetInventory(new List<TalismanItemDefinition>());

            if (itemIds == null)
            {
                return;
            }

            foreach (string itemId in itemIds)
            {
                TalismanItemDefinition definition = FindDefinitionById(itemId);
                if (definition != null)
                {
                    AddTalisman(definition);
                }
            }
        }

        public void CaptureTalismans(MainTrialRuntimeLoadoutSnapshotData snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            snapshot.talismans ??= new List<MainTrialRuntimeTalismanSnapshotData>();
            snapshot.talismans.Clear();
            if (inventory == null)
            {
                return;
            }

            foreach (TalismanItemRuntime item in inventory.GetAllItems())
            {
                if (item?.definition == null || string.IsNullOrWhiteSpace(item.definition.itemId))
                {
                    continue;
                }

                Vector2Int position = item.gridPosition;
                snapshot.talismans.Add(new MainTrialRuntimeTalismanSnapshotData
                {
                    itemId = item.definition.itemId,
                    level = Mathf.Max(1, item.level),
                    isPlaced = item.isPlaced,
                    gridX = item.isPlaced ? position.x : -1,
                    gridY = item.isPlaced ? position.y : -1
                });
            }
        }

        public bool RestoreTalismans(
            MainTrialRuntimeLoadoutSnapshotData snapshot,
            Func<string, TalismanItemDefinition> definitionResolver = null)
        {
            if (snapshot?.talismans == null || snapshot.talismans.Count == 0)
            {
                return false;
            }

            CacheKnownDefinitions();
            GetTemplate();
            ResetTalismansByIds(null);

            bool restoredAny = false;
            bool restoredAllPlacements = true;
            foreach (MainTrialRuntimeTalismanSnapshotData talisman in snapshot.talismans)
            {
                if (talisman == null || string.IsNullOrWhiteSpace(talisman.itemId))
                {
                    continue;
                }

                TalismanItemDefinition definition = definitionResolver?.Invoke(talisman.itemId) ?? FindDefinitionById(talisman.itemId);
                if (definition == null)
                {
                    restoredAllPlacements = false;
                    continue;
                }

                DraggableTalismanItemView view = AddTalisman(definition, talisman.level);
                if (view == null)
                {
                    restoredAllPlacements = false;
                    continue;
                }

                restoredAny = true;
                if (!talisman.isPlaced)
                {
                    continue;
                }

                TalismanGridSlotView slot = FindSlotByPosition(new Vector2Int(talisman.gridX, talisman.gridY));
                if (slot == null || !view.ForcePlaceOnSlot(slot))
                {
                    restoredAllPlacements = false;
                    view.ReturnToHome();
                }
            }

            grid?.NotifyChanged();
            return restoredAny && restoredAllPlacements;
        }

        public TalismanItemDefinition FindDefinitionById(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            CacheKnownDefinitions();
            return definitionCatalog.TryGetValue(itemId, out TalismanItemDefinition definition) ? definition : null;
        }

        private DraggableTalismanItemView GetTemplate()
        {
            if (runtimeTemplate != null)
            {
                return runtimeTemplate;
            }

            if (itemViewTemplate == null)
            {
                return null;
            }

            Transform templateParent = inventoryParent != null ? inventoryParent : itemViewTemplate.transform.parent;
            runtimeTemplate = Instantiate(itemViewTemplate, templateParent);
            runtimeTemplate.name = "RewardItemTemplate_Runtime";
            runtimeTemplate.gameObject.SetActive(false);
            CacheDefinition(itemViewTemplate.Definition);
            return runtimeTemplate;
        }

        private void CacheKnownDefinitions()
        {
            CacheDefinition(itemViewTemplate != null ? itemViewTemplate.Definition : null);
            CacheDefinition(runtimeTemplate != null ? runtimeTemplate.Definition : null);

            if (inventoryParent != null)
            {
                foreach (DraggableTalismanItemView view in inventoryParent.GetComponentsInChildren<DraggableTalismanItemView>(true))
                {
                    CacheDefinition(view != null ? view.Definition : null);
                }
            }

            if (inventory == null)
            {
                return;
            }

            foreach (TalismanItemRuntime item in inventory.GetAllItems())
            {
                CacheDefinition(item != null ? item.definition : null);
            }
        }

        private void CacheDefinition(TalismanItemDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.itemId))
            {
                return;
            }

            definitionCatalog[definition.itemId] = definition;
        }

        private TalismanGridSlotView FindSlotByPosition(Vector2Int position)
        {
            if (grid == null)
            {
                return null;
            }

            foreach (TalismanGridSlotView slot in FindObjectsOfType<TalismanGridSlotView>(true))
            {
                if (slot != null && slot.Grid == grid && slot.GridPosition == position)
                {
                    return slot;
                }
            }

            return null;
        }
    }
}
