using System;
using System.Collections.Generic;
using TalismanBag.Enemies;
using TalismanBag.V02.Config;
using TalismanBag.V02.CoreLoop.Boss;
using TalismanBag.V02.CoreLoop.Rewards;
using TalismanBag.V02.Tags;
using UnityEngine;

namespace TalismanBag.V02.Run
{
    public enum StageType
    {
        Tutorial,
        Normal,
        IdleNormal,
        Boss
    }

    public enum StageWinAction
    {
        NextStage,
        StopBeforeBoss,
        ChapterClearReward,
        ShowHome
    }

    public enum StageLoseAction
    {
        RetrySameStage,
        StayAndPrepare,
        ReturnHome
    }

    [Serializable]
    public sealed class V02RoundConfig
    {
        public string levelId;
        public int roundIndex;
        public string roundTitle;
        public EnemyDefinition enemy;

        [Header("Stage Config Panel 01")]
        public int stageConfigVersion;
        public string chapterId;
        public string nextStageId;
        public StageType stageType;
        public EnemyGroupConfig enemyGroup;
        public RewardConfig rewardConfig;
        public RewardDropTable dropTable;
        public BossInfoConfig bossConfig;
        public string tutorialGuideId;
        public string unlockCondition;
        public StageWinAction onWinAction = StageWinAction.NextStage;
        public StageLoseAction onLoseAction = StageLoseAction.RetrySameStage;
        public bool autoAdvance = true;
        public bool allowBackpackEdit = true;
        public bool stopBeforeBoss;
        public string benchmarkTargetId;

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

        public string StageId => string.IsNullOrWhiteSpace(levelId) ? string.Empty : levelId.Trim();

        public EnemyDefinition ResolveEnemyDefinition()
        {
            return enemyGroup != null && enemyGroup.ResolvePrimaryEnemy() != null
                ? enemyGroup.ResolvePrimaryEnemy()
                : enemy;
        }

        public string ResolveNextStageId()
        {
            return string.IsNullOrWhiteSpace(nextStageId) ? string.Empty : nextStageId.Trim();
        }

        public bool ResolveAutoAdvance(bool legacyFallback)
        {
            return stageConfigVersion > 0 ? autoAdvance : legacyFallback;
        }

        public bool ResolveStopBeforeBoss(bool legacyFallback)
        {
            return stageConfigVersion > 0 ? stopBeforeBoss : legacyFallback;
        }
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
