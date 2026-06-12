using System;
using TalismanBag.Enemies;
using UnityEngine;

namespace TalismanBag.V02.Run
{
    [Serializable]
    public sealed class V02RoundConfig
    {
        public int roundIndex;
        public string roundTitle;
        public EnemyDefinition enemy;

        [TextArea] public string teachingGoal;
        [TextArea] public string preBattleHint;

        public bool isBossRound;

        public float targetDurationMin;
        public float targetDurationMax;
    }
}
