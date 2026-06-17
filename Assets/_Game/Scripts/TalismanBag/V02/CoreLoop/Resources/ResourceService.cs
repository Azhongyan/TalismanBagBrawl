using System;
using TalismanBag.V02.CoreLoop.Save;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Resources
{
    public sealed class ResourceService : MonoBehaviour
    {
        [SerializeField] private SaveService saveService;

        public event Action<ResourceType, int> OnResourceChanged;

        private void Awake()
        {
            EnsureSaveService();
        }

        public void Bind(SaveService service)
        {
            saveService = service;
            EnsureSaveData();
        }

        public int GetAmount(ResourceType type)
        {
            return GetResourceData().GetAmount(type);
        }

        public void Add(ResourceType type, int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            PlayerResourceData resources = GetResourceData();
            int before = resources.GetAmount(type);
            int after = resources.Add(type, amount);
            if (after == before)
            {
                return;
            }

            PersistAndNotify(type, after);
        }

        public bool TrySpend(ResourceType type, int amount)
        {
            if (amount < 0)
            {
                return false;
            }

            PlayerResourceData resources = GetResourceData();
            int before = resources.GetAmount(type);
            if (!resources.TrySpend(type, amount))
            {
                return false;
            }

            int after = resources.GetAmount(type);
            if (after != before)
            {
                PersistAndNotify(type, after);
            }

            return true;
        }

        private PlayerResourceData GetResourceData()
        {
            SaveData saveData = EnsureSaveData();
            saveData.resourceData ??= new PlayerResourceData();
            return saveData.resourceData;
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

        private void PersistAndNotify(ResourceType type, int amount)
        {
            saveService.Save();
            OnResourceChanged?.Invoke(type, amount);
        }
    }
}
