using System;
using UnityEngine;

namespace TalismanBag.V02.Rewards
{
    public sealed class V02RunModifierState : MonoBehaviour
    {
        public bool eyeCoreNineGridUnlocked;
        public bool spiritLinkBetweenStonesUnlocked;
        public bool outerRingDefenseBoostUnlocked;

        public int fireBurnBonusStacks;
        public float thunderShieldBreakMultiplierBonus;
        public float swordCritChanceBonus;
        public float shieldAmountMultiplierBonus;
        public float cleanseCooldownReduction;

        public event Action ModifiersChanged;

        public bool HasAnyModifier =>
            eyeCoreNineGridUnlocked ||
            spiritLinkBetweenStonesUnlocked ||
            outerRingDefenseBoostUnlocked ||
            fireBurnBonusStacks > 0 ||
            thunderShieldBreakMultiplierBonus > 0f ||
            swordCritChanceBonus > 0f ||
            shieldAmountMultiplierBonus > 0f ||
            cleanseCooldownReduction > 0f;

        public void ResetState()
        {
            eyeCoreNineGridUnlocked = false;
            spiritLinkBetweenStonesUnlocked = false;
            outerRingDefenseBoostUnlocked = false;
            fireBurnBonusStacks = 0;
            thunderShieldBreakMultiplierBonus = 0f;
            swordCritChanceBonus = 0f;
            shieldAmountMultiplierBonus = 0f;
            cleanseCooldownReduction = 0f;
            ModifiersChanged?.Invoke();
        }

        public void ApplyReward(V02RewardDefinition reward)
        {
            if (reward == null)
            {
                return;
            }

            switch (reward.rewardType)
            {
                case V02RewardType.FormationModifier:
                    ApplyFormationModifier(reward.formationModifierType);
                    break;
                case V02RewardType.BuildModifier:
                    ApplyBuildModifier(reward.buildModifierType, reward.modifierValue);
                    break;
            }

            ModifiersChanged?.Invoke();
        }

        public bool IsRewardAlreadyApplied(V02RewardDefinition reward)
        {
            if (reward == null)
            {
                return false;
            }

            return reward.rewardType switch
            {
                V02RewardType.FormationModifier => reward.formationModifierType switch
                {
                    V02FormationModifierType.UpgradeEyeCoreToNineGrid => eyeCoreNineGridUnlocked,
                    V02FormationModifierType.SpiritLinkBetweenStones => spiritLinkBetweenStonesUnlocked,
                    V02FormationModifierType.OuterRingDefenseBoost => outerRingDefenseBoostUnlocked,
                    _ => false
                },
                V02RewardType.BuildModifier => reward.buildModifierType switch
                {
                    V02BuildModifierType.FireBurnPlusOne => fireBurnBonusStacks > 0,
                    V02BuildModifierType.ThunderShieldBreakBoost => thunderShieldBreakMultiplierBonus > 0f,
                    V02BuildModifierType.SwordCritBoost => swordCritChanceBonus > 0f,
                    V02BuildModifierType.ShieldAmountBoost => shieldAmountMultiplierBonus > 0f,
                    V02BuildModifierType.CleanseCooldownReduction => cleanseCooldownReduction > 0f,
                    _ => false
                },
                _ => false
            };
        }

        public string BuildDebugSummary()
        {
            return
                $"[奖励强化]\n" +
                $"阵眼九宫：{eyeCoreNineGridUnlocked}\n" +
                $"灵气连线：{spiritLinkBetweenStonesUnlocked}\n" +
                $"外圈护阵：{outerRingDefenseBoostUnlocked}\n" +
                $"火符燃烧：+{fireBurnBonusStacks}\n" +
                $"雷符破盾：+{thunderShieldBreakMultiplierBonus:0.##}\n" +
                $"剑丸暴击：+{swordCritChanceBonus:P0}\n" +
                $"护盾强化：+{shieldAmountMultiplierBonus:P0}\n" +
                $"净化冷却：-{cleanseCooldownReduction:P0}";
        }

        private void ApplyFormationModifier(V02FormationModifierType modifierType)
        {
            switch (modifierType)
            {
                case V02FormationModifierType.UpgradeEyeCoreToNineGrid:
                    eyeCoreNineGridUnlocked = true;
                    break;
                case V02FormationModifierType.SpiritLinkBetweenStones:
                    spiritLinkBetweenStonesUnlocked = true;
                    break;
                case V02FormationModifierType.OuterRingDefenseBoost:
                    outerRingDefenseBoostUnlocked = true;
                    break;
                case V02FormationModifierType.AddSpiritStone:
                    break;
            }
        }

        private void ApplyBuildModifier(V02BuildModifierType modifierType, float value)
        {
            switch (modifierType)
            {
                case V02BuildModifierType.FireBurnPlusOne:
                    fireBurnBonusStacks = Mathf.Max(fireBurnBonusStacks, Mathf.Max(1, Mathf.RoundToInt(value)));
                    break;
                case V02BuildModifierType.ThunderShieldBreakBoost:
                    thunderShieldBreakMultiplierBonus = Mathf.Max(thunderShieldBreakMultiplierBonus, value > 0f ? value : 0.5f);
                    break;
                case V02BuildModifierType.SwordCritBoost:
                    swordCritChanceBonus = Mathf.Max(swordCritChanceBonus, value > 0f ? value : 0.25f);
                    break;
                case V02BuildModifierType.ShieldAmountBoost:
                    shieldAmountMultiplierBonus = Mathf.Max(shieldAmountMultiplierBonus, value > 0f ? value : 0.3f);
                    break;
                case V02BuildModifierType.CleanseCooldownReduction:
                    cleanseCooldownReduction = Mathf.Max(cleanseCooldownReduction, value > 0f ? value : 0.25f);
                    break;
            }
        }
    }
}
