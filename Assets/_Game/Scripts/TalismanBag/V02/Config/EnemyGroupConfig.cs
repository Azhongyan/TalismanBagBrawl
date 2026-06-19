using System;
using System.Collections.Generic;
using TalismanBag.Enemies;
using TalismanBag.V02.EnemySkills;
using UnityEngine;

namespace TalismanBag.V02.Config
{
    [CreateAssetMenu(menuName = "TalismanBag/V02/Stage Config/Enemy Group", fileName = "EnemyGroupConfig")]
    public sealed class EnemyGroupConfig : ScriptableObject
    {
        public string enemyGroupId;
        public string displayName;
        public List<EnemyGroupEntry> entries = new();
        public bool isDebugOnly;
        public bool isDeprecated;
        public CatalogSourceType sourceType = CatalogSourceType.Production;

        public EnemyGroupEntry GetPrimaryEntry()
        {
            EnemyGroupEntry best = null;
            if (entries == null)
            {
                return null;
            }

            foreach (EnemyGroupEntry entry in entries)
            {
                if (entry?.enemy == null || entry.enemyCount <= 0)
                {
                    continue;
                }

                if (best == null || entry.spawnOrder < best.spawnOrder)
                {
                    best = entry;
                }
            }

            return best;
        }

        public EnemyDefinition ResolvePrimaryEnemy()
        {
            return GetPrimaryEntry()?.enemy;
        }

        public EnemyDefinition BuildPrimaryRuntimeEnemy()
        {
            EnemyGroupEntry entry = GetPrimaryEntry();
            EnemyDefinition source = entry?.enemy;
            if (source == null)
            {
                return null;
            }

            EnemyDefinition clone = Instantiate(source);
            clone.hideFlags = HideFlags.DontSave;
            clone.name = $"{source.name}_{enemyGroupId}_Runtime";
            clone.maxHp = Mathf.Max(1, Mathf.RoundToInt(source.maxHp * Mathf.Max(0.01f, entry.hpMultiplier)));
            clone.attackDamage = Mathf.Max(0, Mathf.RoundToInt(source.attackDamage * Mathf.Max(0f, entry.attackMultiplier)));
            clone.attackInterval = Mathf.Max(0.1f, source.attackInterval / Mathf.Max(0.01f, entry.attackSpeedMultiplier));
            clone.baseShield = Mathf.Max(0, Mathf.RoundToInt(source.baseShield * Mathf.Max(0f, entry.shieldMultiplier)));
            clone.skills = CloneSkills(source.skills, entry);
            return clone;
        }

        public string GetCatalogLabel()
        {
            string readableName = string.IsNullOrWhiteSpace(displayName) ? name : displayName.Trim();
            string readableId = string.IsNullOrWhiteSpace(enemyGroupId) ? "no_id" : enemyGroupId.Trim();
            return $"{readableName} [{readableId}]";
        }

        private static List<EnemySkillDefinition> CloneSkills(
            IReadOnlyList<EnemySkillDefinition> sourceSkills,
            EnemyGroupEntry entry)
        {
            List<EnemySkillDefinition> result = new();
            if (sourceSkills == null)
            {
                return result;
            }

            foreach (EnemySkillDefinition sourceSkill in sourceSkills)
            {
                if (sourceSkill == null)
                {
                    continue;
                }

                EnemySkillDefinition clone = Instantiate(sourceSkill);
                clone.hideFlags = HideFlags.DontSave;
                clone.name = $"{sourceSkill.name}_Runtime";
                float frequency = Mathf.Max(0.01f, entry.skillFrequencyMultiplier);
                clone.initialDelay = Mathf.Max(0f, sourceSkill.initialDelay / frequency);
                clone.cooldown = Mathf.Max(0.05f, sourceSkill.cooldown / frequency);
                if (sourceSkill.skillType == EnemySkillType.GainShield)
                {
                    clone.value = Mathf.Max(0, Mathf.RoundToInt(sourceSkill.value * Mathf.Max(0f, entry.shieldMultiplier)));
                }

                result.Add(clone);
            }

            return result;
        }
    }

    [Serializable]
    public sealed class EnemyGroupEntry
    {
        public EnemyDefinition enemy;
        [Min(1)] public int enemyCount = 1;
        public int spawnOrder;
        [Min(0.01f)] public float hpMultiplier = 1f;
        [Min(0f)] public float attackMultiplier = 1f;
        [Min(0.01f)] public float attackSpeedMultiplier = 1f;
        [Min(0f)] public float shieldMultiplier = 1f;
        [Min(0.01f)] public float skillFrequencyMultiplier = 1f;
        public string mechanicTag;

        public string EnemyId => enemy != null ? enemy.enemyId : string.Empty;
    }
}
