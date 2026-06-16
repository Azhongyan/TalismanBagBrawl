using System.Collections.Generic;
using TalismanBag.V02.EnemySkills;
using TalismanBag.V02.Tags;
using UnityEngine;

namespace TalismanBag.Enemies
{
    public enum EnemyType
    {
        Ghost,
        GhostSwarm,
        SwordCultivator,
        EvilCultivator,
        Boss
    }

    [CreateAssetMenu(menuName = "Talisman Bag/Enemy Definition", fileName = "EnemyDefinition")]
    public sealed class EnemyDefinition : ScriptableObject
    {
        public string enemyId;
        public string displayName;
        public bool enabled = true;
        public EnemyType enemyType;
        public int maxHp;
        public int attackDamage;
        public float attackInterval;
        public string weaknessText;
        public string dangerText;
        public string recommendedBuildText;
        public float chargeInterval;
        public float chargeDuration;
        public int chargeAttackDamage;
        public float manaDrainInterval;
        public int manaDrainAmount;
        public float sealInterval;
        public float sealDuration;
        public float ghostShadowInterval;
        public int ghostShadowDamage;
        public Sprite icon;

        [Header("V0.2 Enemy Identity")]
        public string enemyClass;
        public string enemyArchetype;

        [Header("V0.2 Intent")]
        [TextArea] public string intentText;
        [TextArea] public string recommendedCounterText;

        [Header("V0.2 Tags")]
        public List<CounterTag> weaknessTags = new();
        public List<CounterTag> resistTags = new();
        public List<ElementTag> resistedElements = new();
        public List<FunctionTag> resistedFunctions = new();
        public List<ElementTag> vulnerableElements = new();
        public List<FunctionTag> vulnerableFunctions = new();

        [Header("V0.2 Skills")]
        public List<EnemySkillDefinition> skills = new();

        public string GetDisplayName()
        {
            return string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
        }

        public string GetAvatarGlyph()
        {
            string readableName = GetDisplayName();
            return string.IsNullOrWhiteSpace(readableName) ? "?" : readableName[0].ToString();
        }

        public string GetReadableLabel()
        {
            string display = GetDisplayName();
            return $"\u3010{GetAvatarGlyph()}\u3011{display}";
        }
    }
}
