using System.Collections.Generic;
using TalismanBag.V02.Tags;
using UnityEngine;

namespace TalismanBag.V02.EnemySkills
{
    public enum EnemySkillType
    {
        None,
        GainShield,
        SummonMinions,
        ApplyPoison,
        ApplyBurn,
        StealEnergy,
        SealRowOrColumn,
        BossPhaseShield,
        BossPhaseSummon,
        BossPhaseSealEye
    }

    [CreateAssetMenu(menuName = "TalismanBag/V02/Enemy Skill", fileName = "EnemySkillDefinition")]
    public sealed class EnemySkillDefinition : ScriptableObject
    {
        public string skillId;
        public string displayName;

        [TextArea] public string intentText;
        [TextArea] public string effectDescription;

        public EnemySkillType skillType;

        public float initialDelay = 1f;
        public float cooldown = 5f;
        public float castTime = 1f;

        public int value;
        public int duration;

        public List<CounterTag> skillTags = new();
    }
}
