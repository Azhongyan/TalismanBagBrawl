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
        public string levelId;
        public int roundIndex;
        public string roundTitle;
        public EnemyDefinition enemy;

        public string intendedRole;
        [TextArea] public string teachingGoal;
        [TextArea] public string preBattleHint;
        public List<CounterTag> recommendedCounterTags = new();

        public bool isBossRound;

        public float targetDurationMin;
        public float targetDurationMax;
        [Range(0f, 1f)] public float targetHpLossMin;
        [Range(0f, 1f)] public float targetHpLossMax;
        public float strongCounterExpectedDuration;
        public float neutralExpectedDuration;
        public float badBuildExpectedDuration;
        [Range(0f, 1f)] public float expectedPlayerHpRemainStrongCounter = 0.8f;
        [Range(0f, 1f)] public float expectedPlayerHpRemainNeutral = 0.5f;
        [Range(0f, 1f)] public float expectedPlayerHpRemainBadBuild = 0.15f;
        public BenchmarkPassFailRule benchmarkRule = new();
        public List<V02BuildBenchmarkTargetRow> benchmarkTargets = new();

        public string ResolvedIntendedRole => !string.IsNullOrWhiteSpace(intendedRole)
            ? intendedRole
            : !string.IsNullOrWhiteSpace(teachingGoal) ? teachingGoal : roundTitle;
    }

    [Serializable]
    public sealed class BenchmarkPassFailRule
    {
        public float passDurationMax;
        public float weakDurationMax;
        [Range(0f, 1f)] public float passHpLossMax;
        [Range(0f, 1f)] public float weakHpLossMax;
        public int expectedCounterTriggerMin;
        public int expectedSkillCastMin;
    }

    [Serializable]
    public sealed class V02BuildBenchmarkTargetRow
    {
        public string buildId;
        public string expectedGrade;
        public float expectedDurationMin;
        public float expectedDurationMax;
        [TextArea] public string note;
    }
}
