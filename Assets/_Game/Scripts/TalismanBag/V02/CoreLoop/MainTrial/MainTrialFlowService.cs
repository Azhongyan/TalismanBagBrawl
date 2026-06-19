using System.Collections.Generic;
using TalismanBag.V02.CoreLoop.Save;
using TalismanBag.V02.Run;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.MainTrial
{
    public enum MainTrialStartupRouteType
    {
        Home,
        Battle,
        ChapterReward,
        UpgradeRequired,
        BossInfo,
        BossReward,
        CompleteHome
    }

    [System.Serializable]
    public sealed class MainTrialStartupRoute
    {
        public MainTrialStartupRouteType routeType;
        public MainTrialPhase phase;
        public string roundId;
        public string reason;

        public MainTrialStartupRoute(
            MainTrialStartupRouteType routeType,
            MainTrialPhase phase,
            string roundId,
            string reason)
        {
            this.routeType = routeType;
            this.phase = phase;
            this.roundId = roundId ?? string.Empty;
            this.reason = reason ?? string.Empty;
        }
    }

    public sealed class MainTrialFlowService : MonoBehaviour
    {
        private const string ChapterOnePrefix = "1-";
        private const string ChapterTwoPrefix = "2-";
        private const int ChapterTwoBossRound = 10;

        [SerializeField] private SaveService saveService;

        private V02RunConfig chapterTwoRunConfig;

        private void Awake()
        {
            EnsureSaveService();
        }

        public void Bind(SaveService service)
        {
            saveService = service;
            EnsureSaveService();
        }

        public bool ShouldEnterChapterTwoOnStart()
        {
            MainTrialPhase phase = GetCurrentPhase();
            return phase == MainTrialPhase.FirstUpgradeDone ||
                   phase == MainTrialPhase.Chapter2InProgress ||
                   phase == MainTrialPhase.Chapter2BossReady ||
                   phase == MainTrialPhase.Chapter2BossInProgress ||
                   phase == MainTrialPhase.Chapter2Cleared;
        }

        public MainTrialPhase GetCurrentPhase()
        {
            return EnsurePhaseInitialized().mainTrialPhase;
        }

        public MainTrialStartupRoute GetStartupRoute()
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            string currentRoundId = GetCurrentRoundId(progress);
            return progress.mainTrialPhase switch
            {
                MainTrialPhase.NotStarted => CreateRoute(
                    MainTrialStartupRouteType.Battle,
                    progress,
                    "1-1",
                    "New main trial save"),
                MainTrialPhase.Chapter1InProgress => CreateRoute(
                    MainTrialStartupRouteType.Battle,
                    progress,
                    IsRoundInChapter(currentRoundId, ChapterOnePrefix) ? currentRoundId : "1-1",
                    "Resume chapter 1"),
                MainTrialPhase.Chapter1BossCleared => CreateRoute(
                    MainTrialStartupRouteType.ChapterReward,
                    progress,
                    "1-10",
                    "Chapter 1 boss cleared; reward pending"),
                MainTrialPhase.Chapter1RewardClaimed or MainTrialPhase.FirstUpgradeRequired => CreateRoute(
                    MainTrialStartupRouteType.UpgradeRequired,
                    progress,
                    "1-10",
                    "First upgrade required"),
                MainTrialPhase.FirstUpgradeDone => CreateRoute(
                    MainTrialStartupRouteType.Home,
                    progress,
                    "2-1",
                    "First upgrade completed"),
                MainTrialPhase.Chapter2InProgress => CreateRoute(
                    MainTrialStartupRouteType.Battle,
                    progress,
                    IsRoundInChapter(currentRoundId, ChapterTwoPrefix) ? currentRoundId : "2-1",
                    "Resume chapter 2"),
                MainTrialPhase.Chapter2BossReady => CreateRoute(
                    MainTrialStartupRouteType.BossInfo,
                    progress,
                    "2-10",
                    "Chapter 2 boss ready"),
                MainTrialPhase.Chapter2BossInProgress => CreateRoute(
                    MainTrialStartupRouteType.Battle,
                    progress,
                    "2-10",
                    "Resume chapter 2 boss battle"),
                MainTrialPhase.Chapter2Cleared => CreateRoute(
                    MainTrialStartupRouteType.BossReward,
                    progress,
                    "2-10",
                    "Chapter 2 boss cleared; reward pending"),
                MainTrialPhase.CoreLoopComplete => CreateRoute(
                    MainTrialStartupRouteType.CompleteHome,
                    progress,
                    "2-10",
                    "Core loop completed"),
                _ => CreateRoute(
                    MainTrialStartupRouteType.Battle,
                    progress,
                    "1-1",
                    "Unknown phase fallback")
            };
        }

        public V02RunConfig GetOrCreateChapterTwoRunConfig(V02RunConfig chapterOneTemplate)
        {
            if (chapterTwoRunConfig != null)
            {
                return chapterTwoRunConfig;
            }

            chapterTwoRunConfig = ScriptableObject.CreateInstance<V02RunConfig>();
            chapterTwoRunConfig.hideFlags = HideFlags.DontSave;
            chapterTwoRunConfig.runId = "v02_main_trial_2_10";
            chapterTwoRunConfig.displayName = "V0.2 Main Trial 2-10";
            chapterTwoRunConfig.rounds = BuildChapterTwoRounds(chapterOneTemplate);
            return chapterTwoRunConfig;
        }

        public int GetChapterTwoStartIndex(V02RunConfig chapterTwoConfig)
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            int parsedRound = ParseRoundNumber(GetCurrentRoundId(progress));
            int roundNumber = Mathf.Clamp(parsedRound > 0 ? parsedRound : progress.chapterTwoCurrentRoundNumber, 1, ChapterTwoBossRound);
            return Mathf.Clamp(roundNumber - 1, 0, Mathf.Max(0, chapterTwoConfig?.rounds?.Count - 1 ?? 0));
        }

        public bool IsMainTrialRound(V02RoundConfig round)
        {
            return IsChapterOneRound(round) || IsChapterTwoRound(round);
        }

        public bool IsChapterOneRound(V02RoundConfig round)
        {
            return round != null && !string.IsNullOrWhiteSpace(round.levelId) && round.levelId.Trim().StartsWith(ChapterOnePrefix);
        }

        public bool IsChapterTwoRound(V02RoundConfig round)
        {
            return round != null && !string.IsNullOrWhiteSpace(round.levelId) && round.levelId.Trim().StartsWith(ChapterTwoPrefix);
        }

        public bool IsChapterTwoNormalRound(V02RoundConfig round)
        {
            return IsChapterTwoRound(round) && !round.isBossRound && ParseRoundNumber(round.levelId) < ChapterTwoBossRound;
        }

        public bool ShouldAutoStartRound(V02RoundConfig round)
        {
            bool legacyFallback = (IsChapterOneRound(round) && !round.isBossRound) || IsChapterTwoNormalRound(round);
            return round != null && !round.isBossRound && round.ResolveAutoAdvance(legacyFallback);
        }

        public bool ShouldStopBeforeChapterTwoBoss(V02RoundConfig clearedRound)
        {
            bool legacyFallback = IsChapterTwoNormalRound(clearedRound) &&
                                  ParseRoundNumber(clearedRound.levelId) >= ChapterTwoBossRound - 1;
            return clearedRound != null && clearedRound.ResolveStopBeforeBoss(legacyFallback);
        }

        public bool IsChapterTwoBossRound(V02RoundConfig round)
        {
            return IsChapterTwoRound(round) && round.isBossRound && ParseRoundNumber(round.levelId) == ChapterTwoBossRound;
        }

        public bool IsChapterTwoBossDefeated()
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            return progress.mainTrialPhase == MainTrialPhase.Chapter2Cleared ||
                   progress.mainTrialPhase == MainTrialPhase.CoreLoopComplete ||
                   progress.chapterTwoBossCleared ||
                   progress.chapterTwoBossDefeated;
        }

        public void OnChapter1BossWin()
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            progress.mainTrialPhase = MainTrialPhase.Chapter1BossCleared;
            progress.chapterOneBossCleared = true;
            SetCurrentRound(progress, "1-10");
            progress.highestClearedLevelId = "1-10";
            SaveProgress(progress);
        }

        public void OnChapter1RewardClaimed()
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            progress.mainTrialPhase = MainTrialPhase.FirstUpgradeRequired;
            progress.chapterOneBossCleared = true;
            progress.chapterOneBossRewardClaimed = true;
            SetCurrentRound(progress, "1-10");
            SaveProgress(progress);
        }

        public void OnFirstUpgradeCompleted(string itemId = "")
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            progress.mainTrialPhase = MainTrialPhase.FirstUpgradeDone;
            progress.firstUpgradeCompleted = true;
            progress.chapterOneCultivationCompleted = true;
            if (!string.IsNullOrWhiteSpace(itemId))
            {
                progress.firstCultivatedTalismanId = itemId.Trim();
            }

            SetCurrentRound(progress, "2-1");
            progress.highestClearedLevelId = "1-10";
            SaveProgress(progress);
        }

        public void OnChapter2NormalWin(string roundId)
        {
            OnChapter2NormalWin(roundId, string.Empty);
        }

        public void OnChapter2NormalWin(V02RoundConfig round)
        {
            OnChapter2NormalWin(round?.levelId, round?.ResolveNextStageId());
        }

        private void OnChapter2NormalWin(string roundId, string configuredNextStageId)
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            int clearedRoundNumber = Mathf.Clamp(ParseRoundNumber(roundId), 1, ChapterTwoBossRound - 1);
            int nextRoundNumber = Mathf.Clamp(clearedRoundNumber + 1, 1, ChapterTwoBossRound);
            string nextRoundId = string.IsNullOrWhiteSpace(configuredNextStageId)
                ? $"2-{nextRoundNumber}"
                : configuredNextStageId.Trim();
            progress.mainTrialPhase = MainTrialPhase.Chapter2InProgress;
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoCurrentRoundNumber = Mathf.Clamp(ParseRoundNumber(nextRoundId), 1, ChapterTwoBossRound);
            SetCurrentRound(progress, nextRoundId);
            progress.highestClearedLevelId = $"2-{clearedRoundNumber}";
            SaveProgress(progress);
        }

        public void OnChapter2BossReady()
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            progress.mainTrialPhase = MainTrialPhase.Chapter2BossReady;
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoBossUnlocked = true;
            progress.chapterTwoCurrentRoundNumber = ChapterTwoBossRound;
            SetCurrentRound(progress, "2-10");
            SaveProgress(progress);
        }

        public void OnChapter2BossStart()
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            if (progress.mainTrialPhase is MainTrialPhase.Chapter2Cleared or MainTrialPhase.CoreLoopComplete ||
                progress.chapterTwoBossCleared ||
                progress.coreLoopCompleted)
            {
                return;
            }

            progress.mainTrialPhase = MainTrialPhase.Chapter2BossInProgress;
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoBossUnlocked = true;
            progress.chapterTwoCurrentRoundNumber = ChapterTwoBossRound;
            SetCurrentRound(progress, "2-10");
            SaveProgress(progress);
        }

        public void OnChapter2BossWin()
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            if (progress.mainTrialPhase == MainTrialPhase.CoreLoopComplete ||
                progress.coreLoopCompleted)
            {
                return;
            }

            progress.mainTrialPhase = MainTrialPhase.Chapter2Cleared;
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoBossUnlocked = true;
            progress.chapterTwoBossCleared = true;
            progress.chapterTwoBossDefeated = true;
            progress.chapterTwoCurrentRoundNumber = ChapterTwoBossRound;
            SetCurrentRound(progress, "2-10");
            progress.highestClearedLevelId = "2-10";
            SaveProgress(progress);
        }

        public void OnChapter2RewardClaimed()
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            progress.mainTrialPhase = MainTrialPhase.CoreLoopComplete;
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoBossUnlocked = true;
            progress.chapterTwoBossCleared = true;
            progress.chapterTwoBossDefeated = true;
            progress.coreLoopCompleted = true;
            progress.chapterTwoCurrentRoundNumber = ChapterTwoBossRound;
            SetCurrentRound(progress, "2-10");
            progress.highestClearedLevelId = "2-10";
            SaveProgress(progress);
        }

        public void MarkEnteredRound(V02RoundConfig round)
        {
            if (!IsMainTrialRound(round))
            {
                return;
            }

            MainTrialProgressData progress = EnsurePhaseInitialized();
            SetCurrentRound(progress, round.levelId);
            if (IsChapterOneRound(round) &&
                progress.mainTrialPhase is MainTrialPhase.NotStarted or MainTrialPhase.Chapter1InProgress)
            {
                progress.mainTrialPhase = MainTrialPhase.Chapter1InProgress;
            }

            if (IsChapterTwoRound(round))
            {
                progress.chapterTwoUnlocked = true;
                progress.chapterTwoCurrentRoundNumber = Mathf.Clamp(ParseRoundNumber(round.levelId), 1, ChapterTwoBossRound);
                if (!round.isBossRound)
                {
                    progress.mainTrialPhase = MainTrialPhase.Chapter2InProgress;
                }
            }

            SaveProgress(progress);
        }

        public void MarkRoundCleared(V02RoundConfig round)
        {
            if (!IsMainTrialRound(round))
            {
                return;
            }

            if (IsChapterTwoNormalRound(round))
            {
                OnChapter2NormalWin(round);
                return;
            }

            MainTrialProgressData progress = EnsurePhaseInitialized();
            progress.highestClearedLevelId = round.levelId;
            if (IsChapterOneRound(round) && !round.isBossRound)
            {
                progress.mainTrialPhase = MainTrialPhase.Chapter1InProgress;
                int nextRoundNumber = Mathf.Clamp(ParseRoundNumber(round.levelId) + 1, 1, ChapterTwoBossRound);
                string configuredNextStageId = round.ResolveNextStageId();
                SetCurrentRound(
                    progress,
                    string.IsNullOrWhiteSpace(configuredNextStageId)
                        ? $"1-{nextRoundNumber}"
                        : configuredNextStageId);
            }

            SaveProgress(progress);
        }

        public void MarkChapterTwoStarted()
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            progress.mainTrialPhase = MainTrialPhase.Chapter2InProgress;
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoCurrentRoundNumber = Mathf.Max(1, progress.chapterTwoCurrentRoundNumber);
            SetCurrentRound(progress, $"2-{progress.chapterTwoCurrentRoundNumber}");
            SaveProgress(progress);
        }

        public void MarkChapterTwoBossUnlocked()
        {
            OnChapter2BossReady();
        }

        public void MarkChapterTwoBossDefeated()
        {
            OnChapter2BossWin();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public void PrepareChapterOneBossForQa()
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            progress.mainTrialPhase = MainTrialPhase.Chapter1InProgress;
            progress.mainTrialPhaseInitialized = true;
            progress.chapterOneBossCleared = false;
            progress.chapterOneBossRewardClaimed = false;
            progress.firstUpgradeCompleted = false;
            progress.chapterOneCultivationCompleted = false;
            progress.chapterTwoUnlocked = false;
            progress.chapterTwoCurrentRoundNumber = 1;
            progress.chapterTwoBossUnlocked = false;
            progress.chapterTwoBossCleared = false;
            progress.chapterTwoBossDefeated = false;
            progress.coreLoopCompleted = false;
            SetCurrentRound(progress, "1-10");
            progress.highestClearedLevelId = "1-9";
            SaveProgress(progress);
        }

        public void PrepareChapterTwoBossForQa()
        {
            MainTrialProgressData progress = EnsurePhaseInitialized();
            progress.mainTrialPhase = MainTrialPhase.Chapter2BossReady;
            progress.mainTrialPhaseInitialized = true;
            progress.chapterOneBossCleared = true;
            progress.chapterOneBossRewardClaimed = true;
            progress.firstUpgradeCompleted = true;
            progress.chapterOneCultivationCompleted = true;
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoCurrentRoundNumber = ChapterTwoBossRound;
            progress.chapterTwoBossUnlocked = true;
            progress.chapterTwoBossCleared = false;
            progress.chapterTwoBossDefeated = false;
            progress.coreLoopCompleted = false;
            SetCurrentRound(progress, "2-10");
            progress.highestClearedLevelId = "2-9";
            SaveProgress(progress);
        }
#endif

        private List<V02RoundConfig> BuildChapterTwoRounds(V02RunConfig chapterOneTemplate)
        {
            List<V02RoundConfig> rounds = new();
            if (chapterOneTemplate?.chapterTwoRounds != null && chapterOneTemplate.chapterTwoRounds.Count > 0)
            {
                foreach (V02RoundConfig configuredRound in chapterOneTemplate.chapterTwoRounds)
                {
                    if (configuredRound != null)
                    {
                        rounds.Add(CloneConfiguredRound(configuredRound));
                    }
                }

                return rounds;
            }

            if (chapterOneTemplate?.rounds == null || chapterOneTemplate.rounds.Count == 0)
            {
                return rounds;
            }

            int count = Mathf.Min(ChapterTwoBossRound, chapterOneTemplate.rounds.Count);
            for (int i = 0; i < count; i++)
            {
                V02RoundConfig source = chapterOneTemplate.rounds[i];
                if (source == null)
                {
                    continue;
                }

                int roundNumber = i + 1;
                rounds.Add(CloneAsChapterTwoRound(source, roundNumber));
            }

            return rounds;
        }

        private static V02RoundConfig CloneAsChapterTwoRound(V02RoundConfig source, int roundNumber)
        {
            return new V02RoundConfig
            {
                levelId = $"2-{roundNumber}",
                roundIndex = roundNumber,
                roundTitle = source.roundTitle,
                enemy = source.enemy,
                intendedRole = source.intendedRole,
                teachingGoal = roundNumber < ChapterTwoBossRound
                    ? "自动巡行主线：验证培养后的阵容强度。"
                    : source.teachingGoal,
                preBattleHint = roundNumber < ChapterTwoBossRound
                    ? "巡行中会自动开战，普通关胜利后继续前进。"
                    : "前方煞气聚阵，请先查看敌情并整备背包。",
                recommendedCounterTags = source.recommendedCounterTags != null ? new List<TalismanBag.V02.Tags.CounterTag>(source.recommendedCounterTags) : new List<TalismanBag.V02.Tags.CounterTag>(),
                isBossRound = roundNumber == ChapterTwoBossRound,
                targetDurationMin = source.targetDurationMin,
                targetDurationMax = source.targetDurationMax,
                targetHpLossMin = source.targetHpLossMin,
                targetHpLossMax = source.targetHpLossMax,
                strongCounterExpectedDuration = source.strongCounterExpectedDuration,
                neutralExpectedDuration = source.neutralExpectedDuration,
                badBuildExpectedDuration = source.badBuildExpectedDuration,
                expectedPlayerHpRemainStrongCounter = source.expectedPlayerHpRemainStrongCounter,
                expectedPlayerHpRemainNeutral = source.expectedPlayerHpRemainNeutral,
                expectedPlayerHpRemainBadBuild = source.expectedPlayerHpRemainBadBuild,
                benchmarkRule = source.benchmarkRule,
                benchmarkTargets = source.benchmarkTargets != null ? new List<V02BuildBenchmarkTargetRow>(source.benchmarkTargets) : new List<V02BuildBenchmarkTargetRow>()
            };
        }

        private static V02RoundConfig CloneConfiguredRound(V02RoundConfig source)
        {
            return new V02RoundConfig
            {
                levelId = source.levelId,
                roundIndex = source.roundIndex,
                roundTitle = source.roundTitle,
                enemy = source.enemy,
                stageConfigVersion = source.stageConfigVersion,
                chapterId = source.chapterId,
                nextStageId = source.nextStageId,
                stageType = source.stageType,
                enemyGroup = source.enemyGroup,
                rewardConfig = source.rewardConfig,
                dropTable = source.dropTable,
                bossConfig = source.bossConfig,
                tutorialGuideId = source.tutorialGuideId,
                unlockCondition = source.unlockCondition,
                onWinAction = source.onWinAction,
                onLoseAction = source.onLoseAction,
                autoAdvance = source.autoAdvance,
                allowBackpackEdit = source.allowBackpackEdit,
                stopBeforeBoss = source.stopBeforeBoss,
                benchmarkTargetId = source.benchmarkTargetId,
                intendedRole = source.intendedRole,
                teachingGoal = source.teachingGoal,
                preBattleHint = source.preBattleHint,
                recommendedCounterTags = source.recommendedCounterTags != null
                    ? new List<TalismanBag.V02.Tags.CounterTag>(source.recommendedCounterTags)
                    : new List<TalismanBag.V02.Tags.CounterTag>(),
                isBossRound = source.isBossRound,
                targetDurationMin = source.targetDurationMin,
                targetDurationMax = source.targetDurationMax,
                targetHpLossMin = source.targetHpLossMin,
                targetHpLossMax = source.targetHpLossMax,
                strongCounterExpectedDuration = source.strongCounterExpectedDuration,
                neutralExpectedDuration = source.neutralExpectedDuration,
                badBuildExpectedDuration = source.badBuildExpectedDuration,
                expectedPlayerHpRemainStrongCounter = source.expectedPlayerHpRemainStrongCounter,
                expectedPlayerHpRemainNeutral = source.expectedPlayerHpRemainNeutral,
                expectedPlayerHpRemainBadBuild = source.expectedPlayerHpRemainBadBuild,
                benchmarkRule = source.benchmarkRule,
                benchmarkTargets = source.benchmarkTargets != null
                    ? new List<V02BuildBenchmarkTargetRow>(source.benchmarkTargets)
                    : new List<V02BuildBenchmarkTargetRow>()
            };
        }

        private MainTrialProgressData GetProgressData()
        {
            SaveData saveData = EnsureSaveService().EnsureLoaded();
            saveData.mainTrialProgressData ??= new MainTrialProgressData();
            saveData.mainTrialProgressData.Normalize();
            return saveData.mainTrialProgressData;
        }

        private SaveService EnsureSaveService()
        {
            saveService ??= SaveService.GetOrCreate();
            return saveService;
        }

        private MainTrialProgressData EnsurePhaseInitialized()
        {
            SaveService service = EnsureSaveService();
            bool hadPersistedSave = service.HasSave();
            MainTrialProgressData progress = GetProgressData();
            if (progress.mainTrialPhaseInitialized)
            {
                if (EnsureCoreLoopCompleteConsistency(progress) ||
                    EnsureChapterTwoClearedConsistency(progress))
                {
                    SaveProgress(progress);
                }

                return progress;
            }

            progress.mainTrialPhase = hadPersistedSave
                ? MigrateLegacyPhase(progress)
                : MainTrialPhase.NotStarted;
            progress.mainTrialPhaseInitialized = true;
            EnsureCoreLoopCompleteConsistency(progress);
            EnsureChapterTwoClearedConsistency(progress);

            string currentRoundId = GetCurrentRoundId(progress);
            if (string.IsNullOrWhiteSpace(currentRoundId))
            {
                currentRoundId = GetDefaultRoundId(progress.mainTrialPhase);
            }

            SetCurrentRound(progress, currentRoundId);
            SaveProgress(progress);
            Debug.Log(
                $"[V0.2-StateMachine] Migrated phase={progress.mainTrialPhase}, " +
                $"round={progress.currentRoundId}, legacySave={hadPersistedSave}");
            return progress;
        }

        private static bool EnsureCoreLoopCompleteConsistency(MainTrialProgressData progress)
        {
            if (progress.mainTrialPhase != MainTrialPhase.CoreLoopComplete &&
                !progress.coreLoopCompleted)
            {
                return false;
            }

            bool changed = progress.mainTrialPhase != MainTrialPhase.CoreLoopComplete ||
                           !progress.coreLoopCompleted ||
                           !progress.chapterTwoBossCleared ||
                           !progress.chapterTwoBossDefeated ||
                           !progress.chapterTwoBossUnlocked ||
                           !progress.chapterTwoUnlocked ||
                           !string.Equals(
                               GetCurrentRoundId(progress),
                               "2-10",
                               System.StringComparison.OrdinalIgnoreCase);

            progress.mainTrialPhase = MainTrialPhase.CoreLoopComplete;
            progress.coreLoopCompleted = true;
            progress.chapterTwoBossCleared = true;
            progress.chapterTwoBossDefeated = true;
            progress.chapterTwoBossUnlocked = true;
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoCurrentRoundNumber = ChapterTwoBossRound;
            SetCurrentRound(progress, "2-10");

            return changed;
        }

        private static bool EnsureChapterTwoClearedConsistency(MainTrialProgressData progress)
        {
            if (progress.mainTrialPhase == MainTrialPhase.CoreLoopComplete ||
                progress.coreLoopCompleted ||
                (!progress.chapterTwoBossCleared && !progress.chapterTwoBossDefeated))
            {
                return false;
            }

            bool changed = progress.mainTrialPhase != MainTrialPhase.Chapter2Cleared ||
                           !progress.chapterTwoBossCleared ||
                           !progress.chapterTwoBossDefeated ||
                           !progress.chapterTwoBossUnlocked ||
                           !progress.chapterTwoUnlocked ||
                           !string.Equals(
                               GetCurrentRoundId(progress),
                               "2-10",
                               System.StringComparison.OrdinalIgnoreCase);

            progress.mainTrialPhase = MainTrialPhase.Chapter2Cleared;
            progress.chapterTwoBossCleared = true;
            progress.chapterTwoBossDefeated = true;
            progress.chapterTwoBossUnlocked = true;
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoCurrentRoundNumber = ChapterTwoBossRound;
            SetCurrentRound(progress, "2-10");
            progress.highestClearedLevelId = "2-10";
            return changed;
        }

        private static MainTrialPhase MigrateLegacyPhase(MainTrialProgressData progress)
        {
            if (progress.coreLoopCompleted)
            {
                return MainTrialPhase.CoreLoopComplete;
            }

            if (progress.chapterTwoBossCleared || progress.chapterTwoBossDefeated)
            {
                return MainTrialPhase.Chapter2Cleared;
            }

            string currentRoundId = GetCurrentRoundId(progress);
            if (progress.chapterTwoBossUnlocked &&
                string.Equals(currentRoundId, "2-10", System.StringComparison.OrdinalIgnoreCase))
            {
                return MainTrialPhase.Chapter2BossReady;
            }

            if (IsRoundInChapter(currentRoundId, ChapterTwoPrefix) || progress.chapterTwoUnlocked)
            {
                return MainTrialPhase.Chapter2InProgress;
            }

            if (progress.firstUpgradeCompleted || progress.chapterOneCultivationCompleted)
            {
                return MainTrialPhase.FirstUpgradeDone;
            }

            if (progress.chapterOneBossRewardClaimed)
            {
                return MainTrialPhase.FirstUpgradeRequired;
            }

            if (progress.chapterOneBossCleared)
            {
                return MainTrialPhase.Chapter1BossCleared;
            }

            return MainTrialPhase.Chapter1InProgress;
        }

        private static MainTrialStartupRoute CreateRoute(
            MainTrialStartupRouteType routeType,
            MainTrialProgressData progress,
            string roundId,
            string reason)
        {
            return new MainTrialStartupRoute(routeType, progress.mainTrialPhase, roundId, reason);
        }

        private static string GetCurrentRoundId(MainTrialProgressData progress)
        {
            if (progress == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(progress.currentRoundId))
            {
                return progress.currentRoundId.Trim();
            }

            if (!string.IsNullOrWhiteSpace(progress.currentMainTrialLevelId))
            {
                return progress.currentMainTrialLevelId.Trim();
            }

            if (progress.chapterTwoUnlocked)
            {
                return $"2-{Mathf.Clamp(progress.chapterTwoCurrentRoundNumber, 1, ChapterTwoBossRound)}";
            }

            return string.Empty;
        }

        private static string GetDefaultRoundId(MainTrialPhase phase)
        {
            return phase switch
            {
                MainTrialPhase.Chapter1BossCleared or
                MainTrialPhase.Chapter1RewardClaimed or
                MainTrialPhase.FirstUpgradeRequired => "1-10",
                MainTrialPhase.FirstUpgradeDone or MainTrialPhase.Chapter2InProgress => "2-1",
                MainTrialPhase.Chapter2BossReady or
                MainTrialPhase.Chapter2BossInProgress or
                MainTrialPhase.Chapter2Cleared or
                MainTrialPhase.CoreLoopComplete => "2-10",
                _ => "1-1"
            };
        }

        private static void SetCurrentRound(MainTrialProgressData progress, string roundId)
        {
            string normalizedRoundId = string.IsNullOrWhiteSpace(roundId) ? string.Empty : roundId.Trim();
            progress.currentRoundId = normalizedRoundId;
            progress.currentMainTrialLevelId = normalizedRoundId;
        }

        private void SaveProgress(MainTrialProgressData progress)
        {
            progress.mainTrialPhaseInitialized = true;
            progress.Normalize();
            EnsureSaveService().Save();
        }

        private static bool IsRoundInChapter(string roundId, string prefix)
        {
            return !string.IsNullOrWhiteSpace(roundId) &&
                   roundId.Trim().StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase);
        }

        private static int ParseRoundNumber(string levelId)
        {
            if (string.IsNullOrWhiteSpace(levelId))
            {
                return 0;
            }

            string[] parts = levelId.Trim().Split('-');
            if (parts.Length < 2 || !int.TryParse(parts[1], out int roundNumber))
            {
                return 0;
            }

            return roundNumber;
        }
    }
}
