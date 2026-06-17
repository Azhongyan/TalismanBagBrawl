using System.Collections.Generic;
using TalismanBag.V02.CoreLoop.Save;
using TalismanBag.V02.Run;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.MainTrial
{
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
            MainTrialProgressData progress = GetProgressData();
            return progress.chapterOneCultivationCompleted && !progress.chapterTwoBossDefeated;
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
            int roundNumber = Mathf.Clamp(GetProgressData().chapterTwoCurrentRoundNumber, 1, ChapterTwoBossRound);
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
            return IsChapterTwoNormalRound(round);
        }

        public bool ShouldStopBeforeChapterTwoBoss(V02RoundConfig clearedRound)
        {
            return IsChapterTwoNormalRound(clearedRound) && ParseRoundNumber(clearedRound.levelId) >= ChapterTwoBossRound - 1;
        }

        public bool IsChapterTwoBossRound(V02RoundConfig round)
        {
            return IsChapterTwoRound(round) && round.isBossRound && ParseRoundNumber(round.levelId) == ChapterTwoBossRound;
        }

        public bool IsChapterTwoBossDefeated()
        {
            return GetProgressData().chapterTwoBossDefeated;
        }

        public void MarkEnteredRound(V02RoundConfig round)
        {
            if (!IsMainTrialRound(round))
            {
                return;
            }

            MainTrialProgressData progress = GetProgressData();
            progress.currentMainTrialLevelId = round.levelId;
            if (IsChapterTwoRound(round))
            {
                progress.chapterTwoUnlocked = true;
                progress.chapterTwoCurrentRoundNumber = Mathf.Clamp(ParseRoundNumber(round.levelId), 1, ChapterTwoBossRound);
            }

            saveService.Save();
        }

        public void MarkRoundCleared(V02RoundConfig round)
        {
            if (!IsMainTrialRound(round))
            {
                return;
            }

            MainTrialProgressData progress = GetProgressData();
            progress.highestClearedLevelId = round.levelId;
            if (IsChapterTwoRound(round))
            {
                progress.chapterTwoUnlocked = true;
                progress.chapterTwoCurrentRoundNumber = Mathf.Clamp(ParseRoundNumber(round.levelId) + 1, 1, ChapterTwoBossRound);
            }

            saveService.Save();
        }

        public void MarkChapterTwoStarted()
        {
            MainTrialProgressData progress = GetProgressData();
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoCurrentRoundNumber = Mathf.Max(1, progress.chapterTwoCurrentRoundNumber);
            progress.currentMainTrialLevelId = $"2-{progress.chapterTwoCurrentRoundNumber}";
            saveService.Save();
        }

        public void MarkChapterTwoBossUnlocked()
        {
            MainTrialProgressData progress = GetProgressData();
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoBossUnlocked = true;
            progress.chapterTwoCurrentRoundNumber = ChapterTwoBossRound;
            progress.currentMainTrialLevelId = "2-10";
            saveService.Save();
        }

        public void MarkChapterTwoBossDefeated()
        {
            MainTrialProgressData progress = GetProgressData();
            progress.chapterTwoUnlocked = true;
            progress.chapterTwoBossUnlocked = true;
            progress.chapterTwoBossDefeated = true;
            progress.chapterTwoCurrentRoundNumber = ChapterTwoBossRound;
            progress.currentMainTrialLevelId = "2-10";
            progress.highestClearedLevelId = "2-10";
            saveService.Save();
        }

        private List<V02RoundConfig> BuildChapterTwoRounds(V02RunConfig chapterOneTemplate)
        {
            List<V02RoundConfig> rounds = new();
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
