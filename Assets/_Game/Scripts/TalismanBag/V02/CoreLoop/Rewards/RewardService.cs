using System;
using System.Collections.Generic;
using TalismanBag.V02.CoreLoop.Inventory;
using TalismanBag.V02.CoreLoop.Resources;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Rewards
{
    public sealed class RewardService : MonoBehaviour
    {
        [SerializeField] private ResourceService resourceService;
        [SerializeField] private ItemInventoryService itemInventoryService;

        public event Action<RewardResult> OnRewardGranted;

        private void Awake()
        {
            EnsureServices();
        }

        public void Bind(ResourceService resources, ItemInventoryService items)
        {
            resourceService = resources;
            itemInventoryService = items;
            EnsureServices();
        }

        public RewardResult GrantConfig(RewardConfig config)
        {
            if (config == null)
            {
                return CreateEmptyResult(string.Empty, string.Empty);
            }

            return GrantRewards(config.rewards, config.rewardId, config.displayName);
        }

        public RewardResult GrantChapterRewards(RewardConfig config)
        {
            return GrantConfig(config);
        }

        public RewardResult GrantBossClearRewards(RewardConfig config)
        {
            return GrantConfig(config);
        }

        public RewardResult GrantDropTable(RewardDropTable dropTable)
        {
            if (dropTable == null)
            {
                return CreateEmptyResult(string.Empty, string.Empty);
            }

            return GrantRewards(dropTable.Roll(), dropTable.tableId, dropTable.displayName);
        }

        public RewardResult GrantRewards(IReadOnlyList<RewardEntry> rewards, string rewardId = "", string displayName = "")
        {
            EnsureServices();

            RewardResult result = CreateEmptyResult(rewardId, displayName);
            if (rewards == null)
            {
                return result;
            }

            foreach (RewardEntry reward in rewards)
            {
                if (reward == null || !reward.IsValid)
                {
                    continue;
                }

                if (GrantReward(reward))
                {
                    result.rewards.Add(reward.CloneWithAmount(reward.amount));
                }
            }

            if (result.HasRewards)
            {
                OnRewardGranted?.Invoke(result);
            }

            return result;
        }

        private bool GrantReward(RewardEntry reward)
        {
            switch (reward.rewardType)
            {
                case RewardType.Resource:
                    resourceService.Add(reward.resourceType, reward.amount);
                    return true;
                case RewardType.Item:
                    itemInventoryService.AddItem(reward.itemId, reward.amount);
                    return true;
                default:
                    return false;
            }
        }

        private void EnsureServices()
        {
            resourceService ??= FindObjectOfType<ResourceService>(true);
            itemInventoryService ??= FindObjectOfType<ItemInventoryService>(true);

            if (resourceService == null)
            {
                GameObject resourceObject = new("CoreLoopResourceService_Runtime");
                resourceService = resourceObject.AddComponent<ResourceService>();
            }

            if (itemInventoryService == null)
            {
                GameObject inventoryObject = new("CoreLoopItemInventoryService_Runtime");
                itemInventoryService = inventoryObject.AddComponent<ItemInventoryService>();
            }
        }

        private static RewardResult CreateEmptyResult(string rewardId, string displayName)
        {
            return new RewardResult
            {
                rewardId = rewardId,
                displayName = displayName
            };
        }
    }
}
