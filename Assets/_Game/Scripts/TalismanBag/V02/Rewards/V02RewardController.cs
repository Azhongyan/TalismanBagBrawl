using System;
using System.Collections.Generic;
using TalismanBag.Enemies;
using TalismanBag.Inventory;
using TalismanBag.Items;
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
        private const string FireTalismanId = "fire_talisman_basic";
        private const string ShieldTalismanId = "shield_talisman_basic";
        private const string SpiritStoneId = "spirit_stone_basic";
        private const string QiPillId = "qi_pill_basic";
        private const string ThunderTalismanId = "thunder_talisman_basic";
        private const string SwordPillId = "sword_pill_basic";
        private const string ChainThunderId = "chain_thunder_talisman_basic";
        private const string PurifyTalismanId = "purify_talisman_basic";
        private const string SoulSuppressId = "soul_suppress_talisman_basic";
        private const string SealId = "seal_basic";

        private const string RewardAddThunder = "reward_add_thunder_talisman";
        private const string RewardAddSword = "reward_add_sword_pill";
        private const string RewardAddChainThunder = "reward_add_chain_thunder";
        private const string RewardAddPurify = "reward_add_purify_talisman";
        private const string RewardAddSoulSuppress = "reward_add_soul_suppress";
        private const string RewardAddSpiritStone = "reward_add_spirit_stone";
        private const string RewardAddSeal = "reward_add_seal";
        private const string RewardAddQiPill = "reward_add_qi_pill";
        private const string RewardAddBackupOutput = "reward_add_backup_output_talisman";
        private const string RewardFireBurn = "reward_fire_burn_plus_one";
        private const string RewardShieldBoost = "reward_shield_amount_boost";
        private const string RewardCleanseCooldown = "reward_cleanse_cooldown_reduction";
        private const string RewardEyeCore = "reward_upgrade_eye_core_nine_grid";
        private const string RewardThunderBoost = "reward_thunder_shieldbreak_boost";
        private const string RewardChainThunderBoost = "reward_chain_thunder_damage_boost";

        private static readonly string[] StarterItemIds =
        {
            FireTalismanId,
            SpiritStoneId
        };

        [SerializeField] public V02RewardPoolConfig rewardPool;
        [SerializeField] public PlayerTalismanInventory inventory;
        [SerializeField] public FormationPowerResolver formationPowerResolver;
        [SerializeField] public V02RunModifierState runModifierState;
        [SerializeField] public V02RewardPanel rewardPanel;
        [SerializeField] private V02RewardInventoryAdapter inventoryAdapter;
        [SerializeField] private BattleLogUI battleLogUI;
        [SerializeField] public int optionCount = 3;

        private EnemyDefinition currentNextEnemy;
        private int currentCompletedRoundNumber = 1;
        private readonly List<V02RewardDefinition> currentOptions = new();
        private readonly Dictionary<string, V02RewardDefinition> runtimeRewards = new();

        public event Action<V02RewardDefinition> RewardChosen;

        public EnemyDefinition CurrentNextEnemy => currentNextEnemy;

        public void StartNewRewardRun()
        {
            EnsureRuntimeRewards();
            inventoryAdapter?.ResetTalismansByIds(StarterItemIds);
            currentCompletedRoundNumber = 1;
            currentOptions.Clear();
            rewardPanel?.Hide();
        }

        public bool OpenRewardSelection(EnemyDefinition nextEnemy)
        {
            return OpenRewardSelection(nextEnemy, currentCompletedRoundNumber);
        }

        public bool OpenRewardSelection(EnemyDefinition nextEnemy, int completedRoundNumber)
        {
            currentNextEnemy = nextEnemy;
            currentCompletedRoundNumber = Mathf.Clamp(completedRoundNumber, 1, 9);
            currentOptions.Clear();
            currentOptions.AddRange(GenerateRewardOptions(nextEnemy, currentCompletedRoundNumber));
            if (rewardPanel == null)
            {
                battleLogUI?.AddLog($"第 {currentCompletedRoundNumber} 关奖励面板缺失，自动进入下一关");
                return false;
            }

            if (currentOptions.Count == 0)
            {
                battleLogUI?.AddLog($"第 {currentCompletedRoundNumber} 关奖励为空，自动进入下一关");
                return false;
            }

            rewardPanel?.Show(currentOptions, nextEnemy, ChooseReward);
            return true;
        }

        public List<V02RewardDefinition> GenerateRewardOptions(EnemyDefinition nextEnemy)
        {
            return GenerateRewardOptions(nextEnemy, currentCompletedRoundNumber);
        }

        public List<V02RewardDefinition> GenerateRewardOptions(EnemyDefinition nextEnemy, int completedRoundNumber)
        {
            EnsureRuntimeRewards();

            List<V02RewardDefinition> result = new();
            List<V02RewardDefinition> candidates = BuildStageCandidates(completedRoundNumber);
            int targetCount = Mathf.Max(1, optionCount);
            V02RewardDefinition forcedCounterOption = PickForcedCounterOption(candidates, nextEnemy);
            if (forcedCounterOption != null)
            {
                result.Add(forcedCounterOption);
                candidates.Remove(forcedCounterOption);
            }

            while (result.Count < targetCount && candidates.Count > 0)
            {
                V02RewardDefinition selected = PickWeighted(candidates, nextEnemy);
                if (selected == null)
                {
                    break;
                }

                result.Add(selected);
                candidates.Remove(selected);
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

        public void GrantBossCompletionReward()
        {
            TalismanItemDefinition fireTalisman = inventoryAdapter != null ? inventoryAdapter.FindDefinitionById(FireTalismanId) : null;
            if (fireTalisman != null)
            {
                if (inventoryAdapter != null)
                {
                    inventoryAdapter.AddTalisman(fireTalisman);
                }
                else
                {
                    inventory?.AddItem(fireTalisman);
                }
            }

            battleLogUI?.AddLog("Boss 奖励：重复火符、符纸、灵石、基础配方残页、少量修为");
        }

        public bool AddGrantedTalismanToRuntimeInventory(string itemId, int amount = 1)
        {
            TalismanItemDefinition definition = ResolveGrantedTalismanDefinition(itemId);
            if (definition == null)
            {
                battleLogUI?.AddLog($"固定奖励入可拖拽背包失败：未找到道具定义 {itemId}");
                return false;
            }

            int safeAmount = Mathf.Max(1, amount);
            bool addedAny = false;
            for (int i = 0; i < safeAmount; i++)
            {
                if (inventoryAdapter != null)
                {
                    DraggableTalismanItemView view = inventoryAdapter.AddTalisman(definition);
                    if (view == null)
                    {
                        battleLogUI?.AddLog($"Reward inventory sync failed: no draggable item view for {itemId}");
                        return addedAny;
                    }

                    addedAny = true;
                }
                else if (inventory != null)
                {
                    inventory.AddItem(definition);
                    addedAny = true;
                }
                else
                {
                    battleLogUI?.AddLog($"Reward inventory sync failed: no inventory target for {itemId}");
                    return addedAny;
                }
            }

            return addedAny;
        }

        public bool EnsureGrantedTalismanInRuntimeInventory(string itemId, int amount = 1)
        {
            int targetAmount = Mathf.Max(1, amount);
            int currentAmount = GetRuntimeTalismanCount(itemId);
            if (currentAmount >= targetAmount)
            {
                return true;
            }

            return AddGrantedTalismanToRuntimeInventory(itemId, targetAmount - currentAmount);
        }

        public int GetRuntimeTalismanCount(string itemId)
        {
            if (inventory == null || string.IsNullOrWhiteSpace(itemId))
            {
                return 0;
            }

            return inventory.GetItemsByItemId(itemId.Trim()).Count;
        }

        public int GetAdjustedWeight(V02RewardDefinition reward, EnemyDefinition nextEnemy)
        {
            if (reward == null || !reward.enabled)
            {
                return 0;
            }

            int weight = Mathf.Max(0, reward.baseWeight);
            if (RewardHelpsAgainstEnemy(reward, nextEnemy))
            {
                weight += Mathf.Max(0, reward.nextEnemyBonusWeight);
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

        private List<V02RewardDefinition> BuildStageCandidates(int completedRoundNumber)
        {
            List<V02RewardDefinition> candidates = new();
            foreach (string rewardId in GetStageRewardIds(completedRoundNumber))
            {
                V02RewardDefinition reward = FindReward(rewardId);
                if (reward == null || !reward.enabled || ContainsReward(candidates, reward.rewardId))
                {
                    continue;
                }

                if (runModifierState != null &&
                    reward.rewardType != V02RewardType.NewTalisman &&
                    runModifierState.IsRewardAlreadyApplied(reward))
                {
                    continue;
                }

                candidates.Add(reward);
            }

            return candidates;
        }

        private static IEnumerable<string> GetStageRewardIds(int completedRoundNumber)
        {
            switch (Mathf.Clamp(completedRoundNumber, 1, 9))
            {
                case 1:
                    yield return RewardAddThunder;
                    yield return RewardAddSword;
                    yield return RewardAddSeal;
                    break;
                case 2:
                    yield return RewardAddChainThunder;
                    yield return RewardFireBurn;
                    yield return RewardAddSpiritStone;
                    break;
                case 3:
                    yield return RewardAddPurify;
                    yield return RewardShieldBoost;
                    yield return RewardAddQiPill;
                    break;
                case 4:
                    yield return RewardAddSoulSuppress;
                    yield return RewardAddSpiritStone;
                    yield return RewardAddSeal;
                    break;
                case 5:
                    yield return RewardCleanseCooldown;
                    yield return RewardAddBackupOutput;
                    yield return RewardEyeCore;
                    break;
                case 6:
                    yield return RewardThunderBoost;
                    yield return RewardChainThunderBoost;
                    yield return RewardShieldBoost;
                    yield return RewardAddSpiritStone;
                    yield return RewardAddSword;
                    break;
                case 7:
                    yield return RewardThunderBoost;
                    yield return RewardAddChainThunder;
                    yield return RewardAddPurify;
                    yield return RewardShieldBoost;
                    yield return RewardAddSpiritStone;
                    break;
                case 8:
                    yield return RewardChainThunderBoost;
                    yield return RewardFireBurn;
                    yield return RewardAddSoulSuppress;
                    yield return RewardAddSpiritStone;
                    yield return RewardAddSeal;
                    break;
                case 9:
                    yield return RewardCleanseCooldown;
                    yield return RewardThunderBoost;
                    yield return RewardEyeCore;
                    yield return RewardAddPurify;
                    yield return RewardAddSpiritStone;
                    break;
            }
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

                        battleLogUI?.AddLog($"获得符箓：{reward.talismanToAdd.displayName}");
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

        private V02RewardDefinition FindReward(string rewardId)
        {
            if (string.IsNullOrWhiteSpace(rewardId))
            {
                return null;
            }

            if (runtimeRewards.TryGetValue(rewardId, out V02RewardDefinition runtimeReward))
            {
                return runtimeReward;
            }

            if (rewardPool?.rewards == null)
            {
                return null;
            }

            foreach (V02RewardDefinition reward in rewardPool.rewards)
            {
                if (reward != null && reward.rewardId == rewardId)
                {
                    return reward;
                }
            }

            return null;
        }

        private TalismanItemDefinition ResolveGrantedTalismanDefinition(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return null;
            }

            string safeItemId = itemId.Trim();
            if (rewardPool?.rewards != null)
            {
                foreach (V02RewardDefinition reward in rewardPool.rewards)
                {
                    TalismanItemDefinition definition = reward?.talismanToAdd;
                    if (definition != null && definition.itemId == safeItemId)
                    {
                        return definition;
                    }
                }
            }

            foreach (V02RewardDefinition reward in runtimeRewards.Values)
            {
                TalismanItemDefinition definition = reward?.talismanToAdd;
                if (definition != null && definition.itemId == safeItemId)
                {
                    return definition;
                }
            }

            return inventoryAdapter != null ? inventoryAdapter.FindDefinitionById(safeItemId) : null;
        }

        private void EnsureRuntimeRewards()
        {
            AddRuntimeNewTalisman(
                RewardAddSeal,
                "获得法印",
                "获得 1 个法印，用来强化相邻符箓。",
                SealId,
                Array.Empty<CounterTag>(),
                new[] { FunctionTag.Enhance },
                10);

            AddRuntimeNewTalisman(
                RewardAddQiPill,
                "获得丹药",
                "获得 1 颗丹药，用来补充续航。",
                QiPillId,
                new[] { CounterTag.Poison, CounterTag.Burn },
                new[] { FunctionTag.Heal },
                10);

            AddRuntimeNewTalisman(
                RewardAddBackupOutput,
                "备用输出符",
                "获得 1 张火符，用来补充基础单体输出。",
                FireTalismanId,
                new[] { CounterTag.Group },
                new[] { FunctionTag.Damage, FunctionTag.Burn },
                10);

            AddRuntimeBuildModifier(
                RewardChainThunderBoost,
                "连锁雷符强化",
                "连锁雷符伤害提高 30%。",
                V02BuildModifierType.ChainThunderDamageBoost,
                0.3f,
                new[] { CounterTag.Group, CounterTag.Summon },
                new[] { FunctionTag.Chain, FunctionTag.AoE },
                10);
        }

        private void AddRuntimeNewTalisman(
            string rewardId,
            string displayName,
            string shortDescription,
            string itemId,
            CounterTag[] helpfulTags,
            FunctionTag[] functionTags,
            int baseWeight)
        {
            if (runtimeRewards.ContainsKey(rewardId))
            {
                return;
            }

            TalismanItemDefinition talisman = inventoryAdapter != null ? inventoryAdapter.FindDefinitionById(itemId) : null;
            if (talisman == null)
            {
                return;
            }

            V02RewardDefinition reward = CreateRuntimeReward(rewardId, displayName, shortDescription, V02RewardType.NewTalisman, baseWeight);
            reward.talismanToAdd = talisman;
            reward.helpfulAgainstTags.AddRange(helpfulTags);
            reward.relatedFunctionTags.AddRange(functionTags);
            runtimeRewards[rewardId] = reward;
        }

        private void AddRuntimeBuildModifier(
            string rewardId,
            string displayName,
            string shortDescription,
            V02BuildModifierType modifierType,
            float modifierValue,
            CounterTag[] helpfulTags,
            FunctionTag[] functionTags,
            int baseWeight)
        {
            if (runtimeRewards.ContainsKey(rewardId))
            {
                return;
            }

            V02RewardDefinition reward = CreateRuntimeReward(rewardId, displayName, shortDescription, V02RewardType.BuildModifier, baseWeight);
            reward.buildModifierType = modifierType;
            reward.modifierValue = modifierValue;
            reward.helpfulAgainstTags.AddRange(helpfulTags);
            reward.relatedFunctionTags.AddRange(functionTags);
            runtimeRewards[rewardId] = reward;
        }

        private static V02RewardDefinition CreateRuntimeReward(string rewardId, string displayName, string shortDescription, V02RewardType rewardType, int baseWeight)
        {
            V02RewardDefinition reward = ScriptableObject.CreateInstance<V02RewardDefinition>();
            reward.hideFlags = HideFlags.DontSave;
            reward.rewardId = rewardId;
            reward.displayName = displayName;
            reward.rewardType = rewardType;
            reward.shortDescription = shortDescription;
            reward.detailedDescription = shortDescription;
            reward.baseWeight = Mathf.Max(1, baseWeight);
            return reward;
        }

        private V02RewardDefinition PickForcedCounterOption(List<V02RewardDefinition> candidates, EnemyDefinition nextEnemy)
        {
            if (candidates == null || nextEnemy == null)
            {
                return null;
            }

            foreach (V02RewardDefinition reward in candidates)
            {
                if (reward != null && reward.enabled && reward.forceAsCounterOption && RewardHelpsAgainstEnemy(reward, nextEnemy))
                {
                    return reward;
                }
            }

            return null;
        }

        private V02RewardDefinition PickWeighted(List<V02RewardDefinition> candidates, EnemyDefinition nextEnemy)
        {
            int totalWeight = 0;
            foreach (V02RewardDefinition reward in candidates)
            {
                totalWeight += Mathf.Max(0, GetAdjustedWeight(reward, nextEnemy));
            }

            int roll = UnityEngine.Random.Range(0, Mathf.Max(1, totalWeight));
            foreach (V02RewardDefinition reward in candidates)
            {
                roll -= Mathf.Max(0, GetAdjustedWeight(reward, nextEnemy));
                if (roll < 0)
                {
                    return reward;
                }
            }

            return candidates.Count > 0 ? candidates[0] : null;
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
