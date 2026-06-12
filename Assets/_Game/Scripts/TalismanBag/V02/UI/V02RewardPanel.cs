using System;
using System.Collections.Generic;
using TalismanBag.Enemies;
using TalismanBag.V02.EnemySkills;
using TalismanBag.V02.Rewards;
using TalismanBag.V02.Tags;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.V02.UI
{
    public sealed class V02RewardPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text titleText;
        [SerializeField] private Text nextEnemyText;
        [SerializeField] private GameObject[] optionCards;
        [SerializeField] private Text[] optionNameTexts;
        [SerializeField] private Text[] optionTypeTexts;
        [SerializeField] private Text[] optionDescriptionTexts;
        [SerializeField] private Text[] optionTagTexts;
        [SerializeField] private Text[] optionRecommendTexts;
        [SerializeField] private Button[] optionButtons;

        private readonly List<V02RewardDefinition> currentOptions = new();
        private Action<V02RewardDefinition> chooseCallback;
        private EnemyDefinition nextEnemy;

        private void Awake()
        {
            Hide();
        }

        public void Show(List<V02RewardDefinition> options, EnemyDefinition nextEnemyDefinition, Action<V02RewardDefinition> onChoose)
        {
            currentOptions.Clear();
            if (options != null)
            {
                currentOptions.AddRange(options);
            }

            nextEnemy = nextEnemyDefinition;
            chooseCallback = onChoose;

            if (panel != null)
            {
                panel.SetActive(true);
            }

            if (titleText != null)
            {
                titleText.text = "选择奖励";
            }

            if (nextEnemyText != null)
            {
                nextEnemyText.text = nextEnemy != null
                    ? $"下一关：{nextEnemy.displayName}"
                    : "下一关：测试敌人";
            }

            RefreshOptions();
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void RefreshOptions()
        {
            int cardCount = optionCards != null ? optionCards.Length : 0;
            for (int i = 0; i < cardCount; i++)
            {
                V02RewardDefinition reward = i < currentOptions.Count ? currentOptions[i] : null;
                if (optionCards[i] != null)
                {
                    optionCards[i].SetActive(reward != null);
                }

                if (reward == null)
                {
                    continue;
                }

                SetText(optionNameTexts, i, reward.displayName);
                SetText(optionTypeTexts, i, GetRewardTypeLabel(reward.rewardType));
                SetText(optionDescriptionTexts, i, reward.shortDescription);
                SetText(optionTagTexts, i, $"适合克制：{TalismanTagUtility.JoinCounterTags(reward.helpfulAgainstTags)}");
                SetText(optionRecommendTexts, i, RewardHelpsNextEnemy(reward) ? "推荐：克制下一关" : "可改变阵法路线");

                if (optionButtons != null && i < optionButtons.Length && optionButtons[i] != null)
                {
                    int capturedIndex = i;
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => Choose(capturedIndex));
                }
            }
        }

        private void Choose(int index)
        {
            if (index < 0 || index >= currentOptions.Count)
            {
                return;
            }

            chooseCallback?.Invoke(currentOptions[index]);
        }

        private bool RewardHelpsNextEnemy(V02RewardDefinition reward)
        {
            if (reward?.helpfulAgainstTags == null || nextEnemy == null)
            {
                return false;
            }

            foreach (CounterTag tag in reward.helpfulAgainstTags)
            {
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

        private static void SetText(Text[] texts, int index, string value)
        {
            if (texts != null && index < texts.Length && texts[index] != null)
            {
                texts[index].text = value;
            }
        }

        private static string GetRewardTypeLabel(V02RewardType rewardType)
        {
            return rewardType switch
            {
                V02RewardType.NewTalisman => "新符箓",
                V02RewardType.FormationModifier => "阵法改造",
                V02RewardType.BuildModifier => "流派强化",
                _ => "奖励"
            };
        }
    }
}
