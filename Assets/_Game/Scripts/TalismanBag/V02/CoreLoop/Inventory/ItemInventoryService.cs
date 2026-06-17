using System;
using TalismanBag.V02.CoreLoop.Save;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Inventory
{
    public sealed class ItemInventoryService : MonoBehaviour
    {
        [SerializeField] private SaveService saveService;

        public event Action<string, int> OnItemChanged;

        private void Awake()
        {
            EnsureSaveService();
        }

        public void Bind(SaveService service)
        {
            saveService = service;
            EnsureSaveData();
        }

        public void AddItem(string itemId, int amount)
        {
            string safeItemId = NormalizeItemId(itemId);
            if (string.IsNullOrEmpty(safeItemId) || amount <= 0)
            {
                return;
            }

            PlayerItemInventoryData inventoryData = GetInventoryData();
            int before = inventoryData.GetAmount(safeItemId);
            int after = inventoryData.AddItem(safeItemId, amount);
            if (after == before)
            {
                return;
            }

            PersistAndNotify(safeItemId, after);
        }

        public bool HasItem(string itemId)
        {
            return GetInventoryData().HasItem(itemId);
        }

        public int GetAmount(string itemId)
        {
            return GetInventoryData().GetAmount(itemId);
        }

        private PlayerItemInventoryData GetInventoryData()
        {
            SaveData saveData = EnsureSaveData();
            saveData.itemInventoryData ??= new PlayerItemInventoryData();
            return saveData.itemInventoryData;
        }

        private SaveData EnsureSaveData()
        {
            EnsureSaveService();
            return saveService.EnsureLoaded();
        }

        private void EnsureSaveService()
        {
            saveService ??= SaveService.GetOrCreate();
        }

        private void PersistAndNotify(string itemId, int amount)
        {
            saveService.Save();
            OnItemChanged?.Invoke(itemId, amount);
        }

        private static string NormalizeItemId(string itemId)
        {
            return string.IsNullOrWhiteSpace(itemId) ? string.Empty : itemId.Trim();
        }
    }
}
