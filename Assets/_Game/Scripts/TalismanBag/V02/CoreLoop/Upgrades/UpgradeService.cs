using System;
using System.Collections.Generic;
using TalismanBag.V02.CoreLoop.Resources;
using TalismanBag.V02.CoreLoop.Save;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Upgrades
{
    public sealed class UpgradeService : MonoBehaviour
    {
        [SerializeField] private SaveService saveService;
        [SerializeField] private ResourceService resourceService;
        [SerializeField] private TalismanUpgradeConfig upgradeConfig;

        private TalismanUpgradeConfig runtimeDefaultConfig;

        public event Action<string, int> OnTalismanLevelChanged;

        private void Awake()
        {
            EnsureSaveService();
            EnsureResourceService();
            EnsureUpgradeConfig();
        }

        public void Bind(SaveService saveServiceOverride, ResourceService resourceServiceOverride, TalismanUpgradeConfig upgradeConfigOverride = null)
        {
            saveService = saveServiceOverride;
            resourceService = resourceServiceOverride;
            if (upgradeConfigOverride != null)
            {
                upgradeConfig = upgradeConfigOverride;
            }

            EnsureSaveService();
            EnsureResourceService();
            EnsureUpgradeConfig();
        }

        public int GetLevel(string itemId)
        {
            return GetTalismanProgressData().GetLevel(itemId);
        }

        public TalismanLevelConfig GetNextUpgrade(string itemId)
        {
            int currentLevel = GetLevel(itemId);
            return EnsureUpgradeConfig().FindUpgrade(itemId, currentLevel);
        }

        public int GetResourceAmount(ResourceType resourceType)
        {
            return EnsureResourceService().GetAmount(resourceType);
        }

        public bool CanUpgrade(string itemId, out string failureReason)
        {
            failureReason = string.Empty;
            int currentLevel = GetLevel(itemId);
            TalismanLevelConfig levelConfig = EnsureUpgradeConfig().FindUpgrade(itemId, currentLevel);
            if (levelConfig == null)
            {
                failureReason = "当前符箓没有可用培养配置";
                return false;
            }

            if (!HasEnoughResources(levelConfig, out ResourceCost missingCost))
            {
                failureReason = $"{GetResourceDisplayName(missingCost.resourceType)}不足";
                return false;
            }

            return true;
        }

        public TalismanUpgradeResult TryUpgrade(string itemId)
        {
            int currentLevel = GetLevel(itemId);
            TalismanLevelConfig levelConfig = EnsureUpgradeConfig().FindUpgrade(itemId, currentLevel);
            if (levelConfig == null)
            {
                return TalismanUpgradeResult.Failed(itemId, currentLevel, "当前符箓没有可用培养配置");
            }

            if (!HasEnoughResources(levelConfig, out ResourceCost missingCost))
            {
                return TalismanUpgradeResult.Failed(itemId, currentLevel, $"{GetResourceDisplayName(missingCost.resourceType)}不足");
            }

            ResourceService resources = EnsureResourceService();
            foreach (ResourceCost cost in levelConfig.costs)
            {
                if (!cost.IsValid)
                {
                    continue;
                }

                if (!resources.TrySpend(cost.resourceType, cost.amount))
                {
                    return TalismanUpgradeResult.Failed(itemId, currentLevel, $"{GetResourceDisplayName(cost.resourceType)}扣除失败");
                }
            }

            PlayerTalismanProgressData progressData = GetTalismanProgressData();
            progressData.SetLevel(itemId, levelConfig.toLevel);
            saveService.Save();
            OnTalismanLevelChanged?.Invoke(itemId, levelConfig.toLevel);
            return TalismanUpgradeResult.Succeeded(itemId, currentLevel, levelConfig.toLevel, levelConfig);
        }

        public IReadOnlyList<TalismanLevelConfig> GetConfiguredLevels()
        {
            return EnsureUpgradeConfig().levels;
        }

        public static string GetResourceDisplayName(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.SpiritStone => "灵石",
                ResourceType.TalismanPaper => "符纸",
                ResourceType.Cinnabar => "朱砂",
                ResourceType.BasicTalismanEmbryo => "初阶符胚",
                ResourceType.Cultivation => "修为",
                _ => resourceType.ToString()
            };
        }

        private bool HasEnoughResources(TalismanLevelConfig levelConfig, out ResourceCost missingCost)
        {
            ResourceService resources = EnsureResourceService();
            if (levelConfig?.costs != null)
            {
                foreach (ResourceCost cost in levelConfig.costs)
                {
                    if (!cost.IsValid)
                    {
                        continue;
                    }

                    if (resources.GetAmount(cost.resourceType) < cost.amount)
                    {
                        missingCost = cost;
                        return false;
                    }
                }
            }

            missingCost = default;
            return true;
        }

        private PlayerTalismanProgressData GetTalismanProgressData()
        {
            SaveData saveData = EnsureSaveService().EnsureLoaded();
            saveData.talismanProgressData ??= new PlayerTalismanProgressData();
            saveData.talismanProgressData.Normalize();
            return saveData.talismanProgressData;
        }

        private SaveService EnsureSaveService()
        {
            saveService ??= SaveService.GetOrCreate();
            return saveService;
        }

        private ResourceService EnsureResourceService()
        {
            if (resourceService != null)
            {
                return resourceService;
            }

            resourceService = FindObjectOfType<ResourceService>(true);
            if (resourceService != null)
            {
                resourceService.Bind(EnsureSaveService());
                return resourceService;
            }

            GameObject serviceObject = new("CoreLoopResourceService_Runtime");
            resourceService = serviceObject.AddComponent<ResourceService>();
            resourceService.Bind(EnsureSaveService());
            return resourceService;
        }

        private TalismanUpgradeConfig EnsureUpgradeConfig()
        {
            if (upgradeConfig != null)
            {
                return upgradeConfig;
            }

            upgradeConfig = UnityEngine.Resources.Load<TalismanUpgradeConfig>(TalismanUpgradeConfig.DefaultResourcePath);
            if (upgradeConfig != null)
            {
                return upgradeConfig;
            }

            runtimeDefaultConfig ??= TalismanUpgradeConfig.CreateRuntimeDefault();
            return runtimeDefaultConfig;
        }
    }
}
