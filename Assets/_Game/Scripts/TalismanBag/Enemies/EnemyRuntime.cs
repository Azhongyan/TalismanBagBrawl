using System;
using System.Collections.Generic;
using TalismanBag.V02.EnemySkills;

namespace TalismanBag.Enemies
{
    [Serializable]
    public sealed class EnemyRuntime
    {
        public EnemyDefinition definition;
        public int currentHp;
        public bool isBoss;
        public float attackTimer;
        public bool isCharging;
        public bool isEnraged;
        public float chargeTimer;
        public float chargeIntervalTimer;
        public float manaDrainTimer;
        public float specialTimer;
        public float sealTimer;
        public float bossChargeCooldown;
        public bool hasTriggeredEnrage;

        public List<EnemySkillRuntime> skillRuntimes = new();

        public int currentShield;

        public string currentIntentText;
        public bool isCastingSkill;
        public EnemySkillRuntime currentCastingSkill;

        public EnemyRuntime(EnemyDefinition definition)
        {
            Reset(definition);
        }

        public void Reset(EnemyDefinition enemyDefinition)
        {
            definition = enemyDefinition;
            currentHp = definition != null ? definition.maxHp : 1;
            isBoss = definition != null && definition.enemyType == EnemyType.Boss;
            attackTimer = definition != null ? definition.attackInterval : 1f;
            isCharging = false;
            isEnraged = false;
            chargeTimer = 0f;
            chargeIntervalTimer = 0f;
            manaDrainTimer = 0f;
            specialTimer = 0f;
            sealTimer = 0f;
            bossChargeCooldown = 0f;
            hasTriggeredEnrage = false;
            currentShield = 0;
            currentIntentText = definition != null ? definition.intentText : string.Empty;
            isCastingSkill = false;
            currentCastingSkill = null;
            skillRuntimes.Clear();
            if (definition?.skills != null)
            {
                foreach (EnemySkillDefinition skill in definition.skills)
                {
                    if (skill != null)
                    {
                        skillRuntimes.Add(new EnemySkillRuntime(skill));
                    }
                }
            }
        }
    }
}
