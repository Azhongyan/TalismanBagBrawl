using System.Collections.Generic;
using TalismanBag.Items;
using TalismanBag.V02.Tags;
using UnityEngine;

namespace TalismanBag.V02.Rewards
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/Reward", fileName = "V02RewardDefinition")]
    public sealed class V02RewardDefinition : ScriptableObject
    {
        public string rewardId;
        public string displayName;
        public bool enabled = true;

        public V02RewardType rewardType;

        [TextArea] public string shortDescription;
        [TextArea] public string detailedDescription;

        public Sprite icon;

        [Header("New Talisman")]
        public TalismanItemDefinition talismanToAdd;

        [Header("Formation Modifier")]
        public V02FormationModifierType formationModifierType;

        [Header("Build Modifier")]
        public V02BuildModifierType buildModifierType;
        public float modifierValue;

        [Header("Tags")]
        public List<CounterTag> helpfulAgainstTags = new();
        public List<FunctionTag> relatedFunctionTags = new();

        [Header("Weight")]
        public int baseWeight = 10;
        public int nextEnemyBonusWeight = 24;
        public bool forceAsCounterOption;
    }
}
