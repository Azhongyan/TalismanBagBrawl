using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.V04.ChapterFlow
{
    [Serializable]
    public sealed class V04AcceptedBattleResultRecord
    {
        public string resultId = string.Empty;
        public string requestId = string.Empty;
        public string stageId = string.Empty;
        public string fingerprint = string.Empty;

        public V04AcceptedBattleResultRecord Clone()
        {
            return new V04AcceptedBattleResultRecord
            {
                resultId = resultId,
                requestId = requestId,
                stageId = stageId,
                fingerprint = fingerprint
            };
        }
    }

    [Serializable]
    public sealed class V04ChapterFlowStateSnapshot
    {
        public const string SchemaId = "V04ChapterFlowStateSnapshot.v1";

        public string schemaId = SchemaId;
        public string sessionId = string.Empty;
        public string currentChapterId = string.Empty;
        public string currentStageId = string.Empty;
        public V04ChapterFlowPhase phase = V04ChapterFlowPhase.Disabled;
        public string activeBattleRequestId = string.Empty;
        public string pendingResultId = string.Empty;
        public List<string> unlockedChapterIds = new();
        public List<string> completedStageIds = new();
        public List<string> openedHookIds = new();
        public List<V04AcceptedBattleResultRecord> acceptedResults = new();
        public V04ChapterFlowBattleOutcome lastBattleOutcome = V04ChapterFlowBattleOutcome.Unknown;
        public string lastTransitionId = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
        public bool formalFlow;

        public static V04ChapterFlowStateSnapshot CreateDisabled()
        {
            return new V04ChapterFlowStateSnapshot
            {
                schemaId = SchemaId,
                phase = V04ChapterFlowPhase.Disabled,
                devOnly = true,
                isEnabled = false,
                formalFlow = false
            };
        }

        public V04ChapterFlowStateSnapshot Clone()
        {
            return new V04ChapterFlowStateSnapshot
            {
                schemaId = schemaId,
                sessionId = sessionId,
                currentChapterId = currentChapterId,
                currentStageId = currentStageId,
                phase = phase,
                activeBattleRequestId = activeBattleRequestId,
                pendingResultId = pendingResultId,
                unlockedChapterIds = new List<string>(unlockedChapterIds ?? new List<string>()),
                completedStageIds = new List<string>(completedStageIds ?? new List<string>()),
                openedHookIds = new List<string>(openedHookIds ?? new List<string>()),
                acceptedResults = (acceptedResults ?? new List<V04AcceptedBattleResultRecord>())
                    .Where(record => record != null)
                    .Select(record => record.Clone())
                    .ToList(),
                lastBattleOutcome = lastBattleOutcome,
                lastTransitionId = lastTransitionId,
                devOnly = devOnly,
                isEnabled = isEnabled,
                formalFlow = formalFlow
            };
        }
    }

    public sealed class V04ChapterDefinition
    {
        public readonly string chapterId;
        public readonly int chapterIndex;
        public readonly string firstStageId;
        public readonly string bossStageId;
        public readonly string bossProfileId;
        public readonly bool devOnly;
        public readonly bool isEnabled;
        public readonly bool formalFlow;

        public V04ChapterDefinition(
            string chapterId,
            int chapterIndex,
            string firstStageId,
            string bossStageId,
            string bossProfileId)
        {
            this.chapterId = chapterId;
            this.chapterIndex = chapterIndex;
            this.firstStageId = firstStageId;
            this.bossStageId = bossStageId;
            this.bossProfileId = bossProfileId;
            devOnly = true;
            isEnabled = false;
            formalFlow = false;
        }
    }

    public sealed class V04ChapterStageDefinition
    {
        public readonly string chapterId;
        public readonly int chapterIndex;
        public readonly string stageId;
        public readonly int stageIndex;
        public readonly string encounterNodeId;
        public readonly string zoneId;
        public readonly bool isBossStage;
        public readonly bool stopBeforeBossAfterWin;
        public readonly bool requiresManualBossChallenge;
        public readonly string contentBindingSlotId;
        public readonly string bossProfileId;
        public readonly V04BossResolutionMode bossResolutionMode;
        public readonly string beforeChapterTutorialHookId;
        public readonly string beforeChapterStoryHookId;
        public readonly string beforeStageTutorialHookId;
        public readonly string beforeStageStoryHookId;
        public readonly string afterResultTutorialHookId;
        public readonly string afterResultStoryHookId;
        public readonly string beforeBossTutorialHookId;
        public readonly string beforeBossStoryHookId;
        public readonly string beforeBossChallengeTutorialHookId;
        public readonly string beforeBossChallengeStoryHookId;
        public readonly string afterBossResultTutorialHookId;
        public readonly string afterBossResultStoryHookId;
        public readonly string beforeNextChapterUnlockTutorialHookId;
        public readonly string beforeNextChapterUnlockStoryHookId;
        public readonly string dropCandidateHookId;
        public readonly bool devOnly;
        public readonly bool isEnabled;
        public readonly bool formalFlow;

        public V04ChapterStageDefinition(
            string chapterId,
            int chapterIndex,
            string stageId,
            int stageIndex,
            string encounterNodeId,
            bool isBossStage,
            bool stopBeforeBossAfterWin,
            bool requiresManualBossChallenge,
            string contentBindingSlotId,
            string bossProfileId,
            V04BossResolutionMode bossResolutionMode,
            string beforeChapterTutorialHookId,
            string beforeChapterStoryHookId,
            string beforeStageTutorialHookId,
            string beforeStageStoryHookId,
            string afterResultTutorialHookId,
            string afterResultStoryHookId,
            string beforeBossTutorialHookId,
            string beforeBossStoryHookId,
            string beforeBossChallengeTutorialHookId,
            string beforeBossChallengeStoryHookId,
            string afterBossResultTutorialHookId,
            string afterBossResultStoryHookId,
            string beforeNextChapterUnlockTutorialHookId,
            string beforeNextChapterUnlockStoryHookId,
            string dropCandidateHookId)
        {
            this.chapterId = chapterId;
            this.chapterIndex = chapterIndex;
            this.stageId = stageId;
            this.stageIndex = stageIndex;
            this.encounterNodeId = encounterNodeId;
            zoneId = encounterNodeId;
            this.isBossStage = isBossStage;
            this.stopBeforeBossAfterWin = stopBeforeBossAfterWin;
            this.requiresManualBossChallenge = requiresManualBossChallenge;
            this.contentBindingSlotId = contentBindingSlotId;
            this.bossProfileId = bossProfileId;
            this.bossResolutionMode = bossResolutionMode;
            this.beforeChapterTutorialHookId = beforeChapterTutorialHookId;
            this.beforeChapterStoryHookId = beforeChapterStoryHookId;
            this.beforeStageTutorialHookId = beforeStageTutorialHookId;
            this.beforeStageStoryHookId = beforeStageStoryHookId;
            this.afterResultTutorialHookId = afterResultTutorialHookId;
            this.afterResultStoryHookId = afterResultStoryHookId;
            this.beforeBossTutorialHookId = beforeBossTutorialHookId;
            this.beforeBossStoryHookId = beforeBossStoryHookId;
            this.beforeBossChallengeTutorialHookId = beforeBossChallengeTutorialHookId;
            this.beforeBossChallengeStoryHookId = beforeBossChallengeStoryHookId;
            this.afterBossResultTutorialHookId = afterBossResultTutorialHookId;
            this.afterBossResultStoryHookId = afterBossResultStoryHookId;
            this.beforeNextChapterUnlockTutorialHookId = beforeNextChapterUnlockTutorialHookId;
            this.beforeNextChapterUnlockStoryHookId = beforeNextChapterUnlockStoryHookId;
            this.dropCandidateHookId = dropCandidateHookId;
            devOnly = true;
            isEnabled = false;
            formalFlow = false;
        }

        public IEnumerable<string> EnumerateApplicableHookIds()
        {
            string[] values =
            {
                beforeChapterTutorialHookId,
                beforeChapterStoryHookId,
                beforeStageTutorialHookId,
                beforeStageStoryHookId,
                afterResultTutorialHookId,
                afterResultStoryHookId,
                beforeBossTutorialHookId,
                beforeBossStoryHookId,
                beforeBossChallengeTutorialHookId,
                beforeBossChallengeStoryHookId,
                afterBossResultTutorialHookId,
                afterBossResultStoryHookId,
                beforeNextChapterUnlockTutorialHookId,
                beforeNextChapterUnlockStoryHookId,
                dropCandidateHookId
            };

            return values.Where(value => !string.IsNullOrWhiteSpace(value));
        }
    }
}
