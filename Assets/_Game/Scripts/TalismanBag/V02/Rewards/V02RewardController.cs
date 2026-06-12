using System;
using System.Collections.Generic;
using TalismanBag.Enemies;
using TalismanBag.Inventory;
using TalismanBag.V02.EnemySkills;
using TalismanBag.V02.Formation;
using TalismanBag.V02.Tags;
using TalismanBag.V02.UI;
using TalismanBag.UI;
using UnityEngine;

namespace TalismanBag.V02.Rewards
{
    public sealed class V02RewardController : MonoBehaviour
    {
        [SerializeField] public V02RewardPoolConfig rewardPool;
        [SerializeField] public PlayerTalismanInventory inventory;
        [SerializeField] public FormationPowerResolver formationPowerResolver;
        [SerializeField] public V02RunModifierState runModifierState;
        [SerializeField] public V02RewardPanel rewardPanel;
        [SerializeField] private V02RewardInventoryAdapter inventoryAdapter;
        [SerializeField] private BattleLogUI battleLogUI;
        [SerializeField] public int optionCount = 3;

        private EnemyDefinition currentNextEnemy;
        private readonly List<V02RewardDefinition> currentOptions = new();

        public event Action<V02RewardDefinition> RewardChosen;

        public EnemyDefinition CurrentNextEnemy => currentNextEnemy;

        public void OpenRewardSelection(EnemyDefinition nextEnemy)
        {
            currentNextEnemy = nextEnemy;
            currentOptions.Clear();
            currentOptions.AddRange(GenerateRewardOptions(nextEnemy));
            rewardPanel?.Show(currentOptions, nextEnemy, ChooseReward);
        }

        public List<V02RewardDefinition> GenerateRewardOptions(EnemyDefinition nextEnemy)
        {
            List<V02RewardDefinition> result = new();
            if (rewardPool == null || rewardPool.rewards == null || rewardPool.rewards.Count == 0)
            {
                return result;
            }

            TryAddWeighted(result, nextEnemy, reward => reward.rewardType == V02RewardType.NewTalisman);
            TryAddWeighted(result, nextEnemy, reward => reward.rewardType == V02RewardType.FormationModifier || reward.rewardType == V02RewardType.BuildModifier);

            while (result.Count < Mathf.Max(1, optionCount))
            {
                if (!TryAddWeighted(result, nextEnemy, _ => true))
                {
                    break;
                }
            }

            return result;
        }

        public void ChooseReward(V02RewardDefinition reward)
        {
            if (reward == null)
            {
                return;
            }

            ApplyReward(reward);
            rewardPanel?.Hide();
            RewardChosen?.Invoke(reward);
        }

        public void GrantRewardForDebug(V02RewardDefinition reward)
        {
            if (reward == null)
            {
                return;
            }

            ApplyReward(reward);
        }

        public int GetAdjustedWeight(V02RewardDefinition reward, EnemyDefinition nextEnemy)
        {
            if (reward == null)
            {
                return 0;
            }

            int weight = Mathf.Max(1, reward.baseWeight);
            if (RewardHelpsAgainstEnemy(reward, nextEnemy))
            {
                weight += 24;
            }

            if (reward.rewardType == V02RewardType.FormationModifier)
            {
                weight += 3;
            }

            return weight;
        }

        public bool RewardHelpsAgainstEnemy(V02RewardDefinition reward, EnemyDefinition nextEnemy)
        {
            if (reward?.helpfulAgainstTags == null || nextEnemy == null)
            {
                return false;
            }

            foreach (CounterTag tag in reward.helpfulAgainstTags)
            {
                if (tag == CounterTag.None)
                {
                    continue;
                }

                if (nextEnemy.weaknessTags != null && nextEnemy.weaknessTags.Contains(tag))
                {
                    return true;
                }

                if (nextEnemy.skills == null)
                {
                    continue;
                }

                foreach (EnemySkillDefinition skill in nextEnemy.skills)
                {
                    if (skill?.skillTags != null && skill.skillTags.Contains(tag))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void ApplyReward(V02RewardDefinition reward)
        {
            switch (reward.rewardType)
            {
                case V02RewardType.NewTalisman:
                    if (reward.talismanToAdd != null)
                    {
                        if (inventoryAdapter != null)
                        {
                            inventoryAdapter.AddTalisman(reward.talismanToAdd);
                        }
                        else
                        {
                            inventory?.AddItem(reward.talismanToAdd);
                        }

                        battleLogUI?.AddLog($"获得新符箓：{reward.talismanToAdd.displayName}");
                    }
                    break;
                case V02RewardType.FormationModifier:
                case V02RewardType.BuildModifier:
                    runModifierState?.ApplyReward(reward);
                    formationPowerResolver?.RefreshPowerStates();
                    battleLogUI?.AddLog($"获得奖励：{reward.displayName}");
                    break;
            }
        }

        private bool TryAddWeighted(List<V02RewardDefinition> result, EnemyDefinition nextEnemy, Predicate<V02RewardDefinition> predicate)
        {
            List<V02RewardDefinition> candidates = new();
            foreach (V02RewardDefinition reward in rewardPool.rewards)
            {
                if (reward == null || !predicate(reward) || ContainsReward(result, reward.rewardId))
                {
                    continue;
                }

                if (runModifierState != null && reward.rewardType != V02RewardType.NewTalisman && runModifierState.IsRewardAlreadyApplied(reward))
                {
                    continue;
                }

                candidates.Add(reward);
            }

            if (candidates.Count == 0)
            {
                return false;
            }

            V02RewardDefinition selected = PickWeighted(candidates, nextEnemy);
            if (selected == null)
            {
                return false;
            }

            result.Add(selected);
            return true;
        }

        private V02RewardDefinition PickWeighted(List<V02RewardDefinition> candidates, EnemyDefinition nextEnemy)
        {
            int totalWeight = 0;
            foreach (V02RewardDefinition reward in candidates)
            {
                totalWeight += GetAdjustedWeight(reward, nextEnemy);
            }

            int roll = UnityEngine.Random.Range(0, Mathf.Max(1, totalWeight));
            foreach (V02RewardDefinition reward in candidates)
            {
                roll -= GetAdjustedWeight(reward, nextEnemy);
                if (roll < 0)
                {
                    return reward;
                }
            }

            return candidates[0];
        }

        private static bool ContainsReward(List<V02RewardDefinition> rewards, string rewardId)
        {
            foreach (V02RewardDefinition reward in rewards)
            {
                if (reward != null && reward.rewardId == rewardId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
