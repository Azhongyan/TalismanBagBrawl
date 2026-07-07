using System;
using System.Collections.Generic;

namespace TalismanBag.Contracts.Battle
{
    [Serializable]
    public sealed class BattleEnemySnapshot
    {
        public string enemyId = string.Empty;
        public string bossId = string.Empty;
        public int hp;
        public int shield;
        public int attackDamage;
        public float attackInterval;
        public string castSkillId = string.Empty;
        public float castProgress;
        public List<string> mechanicTags = new();
        public List<string> weaknessHints = new();

        public string devOnlyProfileId = string.Empty;
        public List<string> validationTargetBuilds = new();
        public List<string> exactWeaknessTiming = new();
        public List<string> bossKeyRequirements = new();
        public List<string> developerOnlyDiagnostics = new();
        public bool devOnly = true;
    }
}
