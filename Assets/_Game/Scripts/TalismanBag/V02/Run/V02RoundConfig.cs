using System;
using System.Collections.Generic;
using TalismanBag.Enemies;
using TalismanBag.V02.Tags;
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
        public List<CounterTag> recommendedCounterTags = new();

        public bool isBossRound;

        public float targetDurationMin;
        public float targetDurationMax;
        public float strongCounterExpectedDuration;
        public float neutralExpectedDuration;
        public float badBuildExpectedDuration;
        [Range(0f, 1f)] public float expectedPlayerHpRemainStrongCounter = 0.8f;
        [Range(0f, 1f)] public float expectedPlayerHpRemainNeutral = 0.5f;
        [Range(0f, 1f)] public float expectedPlayerHpRemainBadBuild = 0.15f;
    }
}
